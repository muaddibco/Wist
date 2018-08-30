using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Shards;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistrySyncHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistrySync";

        private readonly Dictionary<byte, RoundDescriptor> _roundDescriptors;
        private readonly BlockingCollection<RegistryBlockBase> _registryBlocks;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IDisposable _syncContextChangedUnsibsciber;
        private readonly object _syncRound = new object();
        private readonly ISyncShardsManager _syncShardsManager;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ICryptoService _cryptoService;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private CancellationToken _cancellationToken;
        private byte _round;

        public TransactionsRegistrySyncHandler(IStatesRepository statesRepository, ISyncShardsManager syncShardsManager, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IHashCalculationsRepository hashCalculationsRepository)
        {
            _roundDescriptors = new Dictionary<byte, RoundDescriptor>();
            _registryBlocks = new BlockingCollection<RegistryBlockBase>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _syncContextChangedUnsibsciber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)SynchronizationStateChanged));
            _syncShardsManager = syncShardsManager;
            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
            _cryptoService = cryptoService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _cancellationToken.Register(() => { _syncContextChangedUnsibsciber.Dispose(); });

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            RegistryBlockBase registryBlock = blockBase as RegistryBlockBase;

            if (registryBlock is TransactionsFullBlock || registryBlock is TransactionsRegistryConfidenceBlock)
            {
                _registryBlocks.Add(registryBlock);
            }
        }

        #region Private Functions

        private void SynchronizationStateChanged(string propName)
        {
            _round = 0;
        }

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (RegistryBlockBase registryBlock in _registryBlocks.GetConsumingEnumerable(ct))
            {
                TransactionsFullBlock transactionsFullBlock = registryBlock as TransactionsFullBlock;
                TransactionsRegistryConfidenceBlock transactionsRegistryConfidenceBlock = registryBlock as TransactionsRegistryConfidenceBlock;

                if(transactionsFullBlock != null)
                {
                    lock(_syncRound)
                    {
                        if(transactionsFullBlock.BlockHeight != _round)
                        {
                            continue;
                        }

                        if(!_roundDescriptors.ContainsKey(_round))
                        {
                            RoundDescriptor roundDescriptor = new RoundDescriptor(new Timer(new TimerCallback(RoundEndedHandler), null, 3000, Timeout.Infinite));
                            roundDescriptor.CandidateBlocks.Add(transactionsFullBlock, 0);
                            _roundDescriptors.Add(_round, roundDescriptor);
                        }
                        else
                        {
                            _roundDescriptors[_round].CandidateBlocks.Add(transactionsFullBlock, 0);
                        }
                    }
                }
                else if(transactionsRegistryConfidenceBlock != null)
                {
                    lock(_syncRound)
                    {
                        if(transactionsRegistryConfidenceBlock.BlockHeight != _round)
                        {
                            continue;
                        }

                        if (!_roundDescriptors.ContainsKey(_round))
                        {
                            RoundDescriptor roundDescriptor = new RoundDescriptor(new Timer(new TimerCallback(RoundEndedHandler), null, 3000, Timeout.Infinite));
                            roundDescriptor.VotingBlocks.Add(transactionsRegistryConfidenceBlock);
                            _roundDescriptors.Add(_round, roundDescriptor);
                        }
                        else
                        {
                            _roundDescriptors[_round].VotingBlocks.Add(transactionsRegistryConfidenceBlock);
                        }
                    }
                }
            }
        }

        private void RoundEndedHandler(object state)
        {
            lock(_syncRound)
            {
                RoundDescriptor roundDescriptor = _roundDescriptors[_round];

                Dictionary<IKey, TransactionsFullBlock> keyToFullBlockMap = new Dictionary<IKey, TransactionsFullBlock>();

                foreach (TransactionsFullBlock transactionsFullBlock in roundDescriptor.CandidateBlocks.Keys)
                {
                    TransactionsShortBlock transactionsShortBlock = new TransactionsShortBlock
                    {
                        SyncBlockHeight = transactionsFullBlock.SyncBlockHeight,
                        BlockHeight = transactionsFullBlock.BlockHeight,
                        TransactionHeaderHashes = new SortedList<ushort, IKey>(transactionsFullBlock.TransactionHeaders.ToDictionary(i => i.Key, i => i.Value.GetTransactionRegistryHashKey(_cryptoService, _transactionHashKey)))
                    };

                    ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionsShortBlock);
                    signatureSupportSerializer.FillBodyAndRowBytes();

                    byte[] hash = _defaultTransactionHashCalculation.CalculateHash(transactionsShortBlock.BodyBytes);
                    IKey key = _transactionHashKey.GetKey(hash);
                    keyToFullBlockMap.Add(key, transactionsFullBlock);
                }

                foreach (var confidenceBlock in roundDescriptor.VotingBlocks)
                {
                    IKey key = _transactionHashKey.GetKey(confidenceBlock.ReferencedBlockHash);
                    if (keyToFullBlockMap.ContainsKey(key))
                    {
                        TransactionsFullBlock transactionsFullBlock = keyToFullBlockMap[key];
                        roundDescriptor.CandidateBlocks[transactionsFullBlock] += confidenceBlock.Confidence;
                    }
                }

                TransactionsFullBlock transactionsFullBlockMostConfident = roundDescriptor.CandidateBlocks.OrderByDescending(kv => (double)kv.Value / (double)kv.Key.TransactionHeaders.Count).First().Key;

                ShardDescriptor shardDescriptor = _syncShardsManager.GetShardDescriptorByRound(_round);

                
            }
        }

        #endregion Private Functions

        internal class RoundDescriptor
        {
            public RoundDescriptor(Timer timer)
            {
                CandidateBlocks = new Dictionary<TransactionsFullBlock, int>();
                VotingBlocks = new HashSet<TransactionsRegistryConfidenceBlock>();
                Timer = timer;
            }

            internal Dictionary<TransactionsFullBlock, int> CandidateBlocks { get; }
            internal HashSet<TransactionsRegistryConfidenceBlock> VotingBlocks { get; }
            internal Timer Timer { get; }
        }
    }
}
