using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Serializers;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISyncRegistryMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class SyncRegistryMemPool : ISyncRegistryMemPool
    {
        private readonly object _syncRound = new object();
        private readonly Dictionary<byte, RoundDescriptor> _roundDescriptors;
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly IIdentityKeyProvider _transactionHashKey;

        private readonly Subject<RoundDescriptor> _subject = new Subject<RoundDescriptor>();
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly ICryptoService _cryptoService;
        private byte _round;

        public SyncRegistryMemPool(ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IHashCalculationsRepository hashCalculationsRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService)
        {
            _roundDescriptors = new Dictionary<byte, RoundDescriptor>();
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _cryptoService = cryptoService;
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
        }

        public void AddCandidateBlock(RegistryFullBlock transactionsFullBlock)
        {
            lock (_syncRound)
            {
                if (transactionsFullBlock.BlockHeight != _round)
                {
                    return;
                }

                if (!_roundDescriptors.ContainsKey(_round))
                {
                    RoundDescriptor roundDescriptor = new RoundDescriptor(new Timer(new TimerCallback(RoundEndedHandler), null, 3000, Timeout.Infinite)) { Round = _round };
                    roundDescriptor.CandidateBlocks.Add(transactionsFullBlock, 0);
                    _roundDescriptors.Add(_round, roundDescriptor);
                }
                else
                {
                    if (!_roundDescriptors[_round].IsFinished)
                    {
                        _roundDescriptors[_round].CandidateBlocks.Add(transactionsFullBlock, 0);
                    }
                }
            }
        }

        public void AddVotingBlock(RegistryConfidenceBlock confidenceBlock)
        {
            lock (_syncRound)
            {
                if (confidenceBlock.BlockHeight != _round)
                {
                    return;
                }

                if (!_roundDescriptors.ContainsKey(_round))
                {
                    RoundDescriptor roundDescriptor = new RoundDescriptor(new Timer(new TimerCallback(RoundEndedHandler), null, 3000, Timeout.Infinite));
                    roundDescriptor.VotingBlocks.Add(confidenceBlock);
                    _roundDescriptors.Add(_round, roundDescriptor);
                }
                else
                {
                    if (!_roundDescriptors[_round].IsFinished)
                    {
                        _roundDescriptors[_round].VotingBlocks.Add(confidenceBlock);
                    }
                }
            }
        }

        public IDisposable SubscribeOnRoundElapsed(ITargetBlock<RoundDescriptor> onRoundElapsed)
        {
            return _subject.Subscribe(onRoundElapsed.AsObserver());
        }

        public void SetRound(byte round)
        {
            lock (_syncRound)
            {
                _round = round;
            }
        }

        public void GetMostConfidentFullBlock(out RegistryFullBlock transactionsFullBlockMostConfident, out IKey mostConfidentKey)
        {
            RoundDescriptor roundDescriptor = _roundDescriptors[_round];

            Dictionary<IKey, RegistryFullBlock> keyToFullBlockMap = new Dictionary<IKey, RegistryFullBlock>();

            foreach (RegistryFullBlock transactionsFullBlock in roundDescriptor.CandidateBlocks.Keys)
            {
                IKey key = GetTransactionKeyForFullBlock(transactionsFullBlock);
                keyToFullBlockMap.Add(key, transactionsFullBlock);
            }

            foreach (var confidenceBlock in roundDescriptor.VotingBlocks)
            {
                IKey key = _transactionHashKey.GetKey(confidenceBlock.ReferencedBlockHash);
                if (keyToFullBlockMap.ContainsKey(key))
                {
                    RegistryFullBlock transactionsFullBlock = keyToFullBlockMap[key];
                    roundDescriptor.CandidateBlocks[transactionsFullBlock] += confidenceBlock.Confidence;
                }
            }

            transactionsFullBlockMostConfident = roundDescriptor.CandidateBlocks.OrderByDescending(kv => (double)kv.Value / (double)kv.Key.TransactionHeaders.Count).First().Key;
            mostConfidentKey = GetTransactionKeyForFullBlock(transactionsFullBlockMostConfident);
        }

        private IKey GetTransactionKeyForFullBlock(RegistryFullBlock transactionsFullBlock)
        {
            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock
            {
                SyncBlockHeight = transactionsFullBlock.SyncBlockHeight,
                BlockHeight = transactionsFullBlock.BlockHeight,
                Nonce = 0,
                PowHash = new byte[Globals.POW_HASH_SIZE],
                TransactionHeaderHashes = new SortedList<ushort, IKey>(transactionsFullBlock.TransactionHeaders.ToDictionary(i => i.Key, i => i.Value.GetTransactionRegistryHashKey(_cryptoService, _transactionHashKey)))
            };

            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionsShortBlock);
            signatureSupportSerializer.FillBodyAndRowBytes();

            byte[] hash = _defaultTransactionHashCalculation.CalculateHash(transactionsShortBlock.BodyBytes);
            IKey key = _transactionHashKey.GetKey(hash);
            return key;
        }

        #region Private Functions

        private void RoundEndedHandler(object state)
        {
            lock (_syncRound)
            {
                try
                {
                    _subject.OnNext(_roundDescriptors[_round]);
                }
                catch (Exception)
                {
                }
                
                _roundDescriptors[_round].IsFinished = true;
            }
        }

        #endregion Private Functions
    }
}
