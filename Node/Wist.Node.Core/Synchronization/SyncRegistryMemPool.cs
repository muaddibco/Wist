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
            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("DefaultHash");
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
                    RoundDescriptor roundDescriptor = new RoundDescriptor(new Timer(new TimerCallback(RoundEndedHandler), null, 3000, Timeout.Infinite), _transactionHashKey) { Round = _round };
                    roundDescriptor.AddFullBlock(transactionsFullBlock);
                    _roundDescriptors.Add(_round, roundDescriptor);
                }
                else
                {
                    if (!_roundDescriptors[_round].IsFinished)
                    {
                        _roundDescriptors[_round].AddFullBlock(transactionsFullBlock);
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
                    RoundDescriptor roundDescriptor = new RoundDescriptor(new Timer(new TimerCallback(RoundEndedHandler), null, 3000, Timeout.Infinite), _transactionHashKey);
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

            foreach (var confidenceBlock in roundDescriptor.VotingBlocks)
            {
                IKey key = _transactionHashKey.GetKey(confidenceBlock.ReferencedBlockHash);
                if (roundDescriptor.CandidateVotes.ContainsKey(key))
                {
                    roundDescriptor.CandidateVotes[key] += confidenceBlock.Confidence;
                }
            }

            mostConfidentKey = roundDescriptor.CandidateVotes.OrderByDescending(kv => (double)kv.Value / (double)roundDescriptor.CandidateBlocks[kv.Key].TransactionHeaders.Count).First().Key;
            transactionsFullBlockMostConfident = roundDescriptor.CandidateBlocks[mostConfidentKey];
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
