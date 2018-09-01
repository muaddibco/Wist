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
using Wist.Communication.Interfaces;
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

        private readonly BlockingCollection<RegistryBlockBase> _registryBlocks;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IDisposable _syncContextChangedUnsibsciber;
        private readonly ISyncShardsManager _syncShardsManager;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ICryptoService _cryptoService;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly ISyncRegistryNeighborhoodState _syncRegistryNeighborhoodState;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly ISyncRegistryMemPool _syncRegistryMemPool;
        private IServerCommunicationService _communicationService;
        private CancellationToken _cancellationToken;

        public TransactionsRegistrySyncHandler(IStatesRepository statesRepository, ISyncShardsManager syncShardsManager, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IHashCalculationsRepository hashCalculationsRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, ISyncRegistryMemPool syncRegistryMemPool)
        {
            _registryBlocks = new BlockingCollection<RegistryBlockBase>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _syncRegistryNeighborhoodState = statesRepository.GetInstance<ISyncRegistryNeighborhoodState>();
            _syncContextChangedUnsibsciber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)SynchronizationStateChanged));
            _syncShardsManager = syncShardsManager;
            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
            _cryptoService = cryptoService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _communicationServicesRegistry = communicationServicesRegistry;
            _syncRegistryMemPool = syncRegistryMemPool;
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _cancellationToken.Register(() => { _syncContextChangedUnsibsciber.Dispose(); });
            _communicationService = _communicationServicesRegistry.GetInstance("GenericTcp");

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            RegistryBlockBase registryBlock = blockBase as RegistryBlockBase;

            if (registryBlock is RegistryFullBlock || registryBlock is RegistryConfidenceBlock)
            {
                _registryBlocks.Add(registryBlock);
            }
        }

        #region Private Functions

        private void SynchronizationStateChanged(string propName)
        {
            _syncRegistryMemPool.SetRound(0);
        }

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (RegistryBlockBase registryBlock in _registryBlocks.GetConsumingEnumerable(ct))
            {
                if (registryBlock is RegistryFullBlock transactionsFullBlock)
                {
                    _syncRegistryMemPool.AddCandidateBlock(transactionsFullBlock);
                }
                else if (registryBlock is RegistryConfidenceBlock transactionsRegistryConfidenceBlock)
                {
                    _syncRegistryMemPool.AddVotingBlock(transactionsRegistryConfidenceBlock);
                }
            }
        }

        private void RoundEndedHandler(RoundDescriptor roundDescriptor)
        {
            RegistryFullBlock transactionsFullBlockMostConfident = _syncRegistryMemPool.GetMostConfidentFullBlock();

            RegistryConfirmationBlock registryConfirmationBlock = new RegistryConfirmationBlock
            {
                SyncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0,
                BlockHeight = roundDescriptor.Round,
                ReferencedBlockHash = transactionsFullBlockMostConfident.ShortBlockHash
            };

            ShardDescriptor shardDescriptor = _syncShardsManager.GetShardDescriptorByRound(roundDescriptor.Round);
            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(registryConfirmationBlock);

            _communicationService.PostMessage(_syncRegistryNeighborhoodState.GetAllNeighbors(), signatureSupportSerializer);

            //TODO: transactionsFullBlockMostConfident must be sent to Storage level and combined with other most confident full blocks from other shards
        }

        #endregion Private Functions
    }
}
