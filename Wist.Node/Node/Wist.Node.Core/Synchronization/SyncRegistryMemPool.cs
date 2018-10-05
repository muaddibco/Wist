using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Serializers;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Core.ExtensionMethods;
using Wist.Core.Logging;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISyncRegistryMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class SyncRegistryMemPool : ISyncRegistryMemPool
    {
        private readonly object _syncRound = new object();
        private readonly Dictionary<ulong, RoundDescriptor> _roundDescriptors;
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly IIdentityKeyProvider _transactionHashKey;

        private readonly Subject<RoundDescriptor> _subject = new Subject<RoundDescriptor>();
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly ICryptoService _cryptoService;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ILogger _logger;

        private ulong _lastCompletedSyncHeight = 0;
        private ulong _lastCompletedRound = 0;

        public SyncRegistryMemPool(ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IHashCalculationsRepository hashCalculationsRepository, 
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService, IStatesRepository statesRepository, ILoggerService loggerService)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _roundDescriptors = new Dictionary<ulong, RoundDescriptor>();
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _cryptoService = cryptoService;
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("DefaultHash");
            _logger = loggerService.GetLogger(nameof(SyncRegistryMemPool));
        }

        public void AddCandidateBlock(RegistryFullBlock transactionsFullBlock)
        {
            if (transactionsFullBlock == null)
            {
                throw new ArgumentNullException(nameof(transactionsFullBlock));
            }

            _logger.Debug($"{nameof(SyncRegistryMemPool)} - adding candidate block {transactionsFullBlock.RawData.ToHexString()}");

            if (_lastCompletedRound > 1 && transactionsFullBlock.BlockHeight <= _lastCompletedRound && transactionsFullBlock.BlockHeight != 1 || _lastCompletedRound == 1 && transactionsFullBlock.BlockHeight == 1)
            {
                return;
            }

            lock (_syncRound)
            {
                if (!_roundDescriptors.ContainsKey(transactionsFullBlock.BlockHeight))
                {
                    RoundDescriptor roundDescriptor = new RoundDescriptor(_transactionHashKey);
                    roundDescriptor.AddFullBlock(transactionsFullBlock);
                    _roundDescriptors.Add(transactionsFullBlock.BlockHeight, roundDescriptor);
                }
                else
                {
                    _logger.Debug($"AddCandidateBlock - Number of candidate blocks for round {transactionsFullBlock.BlockHeight} = {_roundDescriptors[transactionsFullBlock.BlockHeight].CandidateBlocks.Count}");
                    if (!_roundDescriptors[transactionsFullBlock.BlockHeight].IsFinished)
                    {
                        _roundDescriptors[transactionsFullBlock.BlockHeight].AddFullBlock(transactionsFullBlock);
                    }
                }

                _logger.Debug($"AddCandidateBlock - Number of candidate blocks for round {transactionsFullBlock.BlockHeight} = {_roundDescriptors[transactionsFullBlock.BlockHeight].CandidateBlocks.Count}");
            }
        }

        public void AddVotingBlock(RegistryConfidenceBlock confidenceBlock)
        {
            if (confidenceBlock == null)
            {
                throw new ArgumentNullException(nameof(confidenceBlock));
            }

            if(confidenceBlock.SyncBlockHeight <= _lastCompletedSyncHeight)
            {
                return;
            }

            lock (_syncRound)
            {
                if (!_roundDescriptors.ContainsKey(confidenceBlock.BlockHeight))
                {
                    RoundDescriptor roundDescriptor = new RoundDescriptor(_transactionHashKey);
                    roundDescriptor.AddVotingBlock(confidenceBlock);
                    _roundDescriptors.Add(confidenceBlock.BlockHeight, roundDescriptor);
                }
                else
                {
                    if (!_roundDescriptors[confidenceBlock.BlockHeight].IsFinished)
                    {
                        _roundDescriptors[confidenceBlock.BlockHeight].AddVotingBlock(confidenceBlock);
                    }
                }
            }
        }

        public RegistryFullBlock GetMostConfidentFullBlock(ulong round)
        {
            if (!_roundDescriptors.ContainsKey(round))
            {
                return null;
            }

            RoundDescriptor roundDescriptor = _roundDescriptors[round];

            foreach (var confidenceBlock in roundDescriptor.VotingBlocks)
            {
                IKey key = _transactionHashKey.GetKey(confidenceBlock.ReferencedBlockHash);
                if (roundDescriptor.CandidateVotes.ContainsKey(key))
                {
                    long sum = GetConfidence(confidenceBlock.BitMask);
                    roundDescriptor.CandidateVotes[key] += (int)sum;
                }
            }

            RegistryFullBlock transactionsFullBlockMostConfident = null;

            if (roundDescriptor.CandidateVotes?.Count > 0 )
            {
                IKey mostConfidentKey = roundDescriptor.CandidateVotes.OrderByDescending(kv => (double)kv.Value / (double)roundDescriptor.CandidateBlocks[kv.Key].TransactionHeaders.Count).First().Key;
                transactionsFullBlockMostConfident = roundDescriptor.CandidateBlocks[mostConfidentKey];
            }

            return transactionsFullBlockMostConfident;
        }

        private static long GetConfidence(byte[] bitMask)
        {
            long sum = 0;
            byte[] numBytes = new byte[8];
            for (int i = 0; i < bitMask.Length; i += 8)
            {
                long num;
                if (bitMask.Length - i < 8)
                {
                    numBytes[0] = 0;
                    numBytes[1] = 0;
                    numBytes[2] = 0;
                    numBytes[3] = 0;
                    numBytes[4] = 0;
                    numBytes[5] = 0;
                    numBytes[6] = 0;
                    numBytes[7] = 0;

                    Array.Copy(bitMask, i, numBytes, 0, bitMask.Length - i);
                    num = BitConverter.ToInt64(numBytes, 0);
                }
                else
                {
                    num = BitConverter.ToInt64(bitMask, i);
                }

                sum += NumberOfSetBits(num);
            }

            return sum;
        }

        #region Private Functions

        private static long NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56;
        }

        public void ResetRound(ulong round)
        {
            _lastCompletedRound = round;

            if(_roundDescriptors.ContainsKey(round))
            {
                _roundDescriptors[round].Reset();
                _logger.Debug($"ResetRound - Number of candidate blocks for round {round} = {_roundDescriptors[round].CandidateBlocks.Count}");
            }
        }

        public void SetLastCompletedSyncHeight(ulong syncHeight)
        {
            _lastCompletedSyncHeight = syncHeight;
        }

        #endregion Private Functions
    }
}
