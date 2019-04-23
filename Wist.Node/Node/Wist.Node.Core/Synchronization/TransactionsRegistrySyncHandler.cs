using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.Core.Serializers;
using Wist.Network.Interfaces;
using Wist.Network.Topology;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Serializers.RawPackets;
using Wist.Core.Logging;
using Wist.Core.ExtensionMethods;
using Wist.Core.Configuration;
using Wist.Core;
using Wist.Core.Models;
using Wist.Node.Core.Common;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistrySyncHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistrySync";

        private readonly int _cyclePeriodMsec = 5000;

        private readonly BlockingCollection<RegistryBlockBase> _registryBlocks;
        private readonly INodeContext _nodeContext;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IDisposable _syncContextChangedUnsibsciber;
        private readonly ISyncShardsManager _syncShardsManager;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ISerializersFactory _serializersFactory;
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly IHashCalculation _powCalculation;
        private readonly ISyncRegistryNeighborhoodState _syncRegistryNeighborhoodState;

        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly ISyncRegistryMemPool _syncRegistryMemPool;
        private readonly INodesResolutionService _nodesResolutionService;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IChainDataService _synchronizationChainDataService;
        private readonly IChainDataService _registryChainDataService;
        private ISynchronizationConfiguration _synchronizationConfiguration;
        private readonly IConfigurationService _configurationService;

        private readonly ILogger _logger;

        private IServerCommunicationService _communicationService;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource = null;

        public TransactionsRegistrySyncHandler(IStatesRepository statesRepository, ISyncShardsManager syncShardsManager, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            ISerializersFactory serializersFactory, IHashCalculationsRepository hashCalculationsRepository, 
            IServerCommunicationServicesRegistry communicationServicesRegistry, ISyncRegistryMemPool syncRegistryMemPool, INodesResolutionService nodesResolutionService,
            IChainDataServicesManager chainDataServicesManager, IRawPacketProvidersFactory rawPacketProvidersFactory,IConfigurationService configurationService, ILoggerService loggerService)
        {
            _configurationService = configurationService;
            _registryBlocks = new BlockingCollection<RegistryBlockBase>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _syncRegistryNeighborhoodState = statesRepository.GetInstance<ISyncRegistryNeighborhoodState>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _syncContextChangedUnsibsciber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)SynchronizationStateChanged));
            _syncShardsManager = syncShardsManager;
            _transactionHashKey = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _serializersFactory = serializersFactory;
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _powCalculation = hashCalculationsRepository.Create(Globals.POW_TYPE);
            _communicationServicesRegistry = communicationServicesRegistry;
            _syncRegistryMemPool = syncRegistryMemPool;
            _nodesResolutionService = nodesResolutionService;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _synchronizationChainDataService = chainDataServicesManager.GetChainDataService(PacketType.Synchronization);
            _registryChainDataService = chainDataServicesManager.GetChainDataService(PacketType.Registry);
            _logger = loggerService.GetLogger(nameof(TransactionsRegistrySyncHandler));
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _cancellationToken.Register(() => { _syncContextChangedUnsibsciber.Dispose(); });
            _synchronizationConfiguration = _configurationService.Get<ISynchronizationConfiguration>();
            _communicationService = _communicationServicesRegistry.GetInstance(_synchronizationConfiguration.CommunicationServiceName);

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(PacketBase blockBase)
        {
            _logger.Debug($"{nameof(TransactionsRegistrySyncHandler)} - processing block {blockBase.RawData.ToHexString()}");
            RegistryBlockBase registryBlock = blockBase as RegistryBlockBase;

            if (registryBlock is RegistryFullBlock)
            {
                _registryBlocks.Add(registryBlock);
            }
        }

        #region Private Functions

        private void SynchronizationStateChanged(string propName)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            PeriodicTaskFactory.Start(() => 
            {
                IEnumerable<RegistryFullBlock> registryFullBlocks = _syncRegistryMemPool.GetRegistryBlocks();

                CreateAndDistributeCombinedBlock(registryFullBlocks);
                DistributeAndSaveFullBlock(registryFullBlocks);
            }, _cyclePeriodMsec * _synchronizationConfiguration.TotalNodes, _cyclePeriodMsec * _synchronizationConfiguration.Position, cancelToken: _cancellationTokenSource.Token, periodicTaskCreationOptions: TaskCreationOptions.LongRunning);
        }

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (RegistryBlockBase registryBlock in _registryBlocks.GetConsumingEnumerable(ct))
            {
                try
                {
                    _logger.Debug($"Obtained block {registryBlock.GetType().Name} with Round {registryBlock.BlockHeight}");

                    lock (_syncRegistryMemPool)
                    {
                        if (registryBlock is RegistryFullBlock transactionsFullBlock)
                        {
                            _syncRegistryMemPool.AddCandidateBlock(transactionsFullBlock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error during processing block", ex);
                }
            }
        }

        private void CreateAndDistributeCombinedBlock(IEnumerable<RegistryFullBlock> registryFullBlocks)
        {
            lock (_synchronizationContext)
            {
                SynchronizationRegistryCombinedBlock lastCombinedBlock = (SynchronizationRegistryCombinedBlock)_synchronizationChainDataService.GetAllLastBlocksByType(BlockTypes.Synchronization_RegistryCombinationBlock).Single();
                byte[] prevHash = lastCombinedBlock != null ? _defaultTransactionHashCalculation.CalculateHash(lastCombinedBlock.RawData) : new byte[Globals.DEFAULT_HASH_SIZE];

                //TODO: For initial POC there will be only one participant at Synchronization Layer, thus combination of FullBlocks won't be implemented fully
                SynchronizationRegistryCombinedBlock synchronizationRegistryCombinedBlock = new SynchronizationRegistryCombinedBlock
                {
                    SyncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0,
                    PowHash = _powCalculation.CalculateHash(_synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE]),
                    BlockHeight = ++_synchronizationContext.LastRegistrationCombinedBlockHeight,
                    HashPrev = prevHash,
                    ReportedTime = DateTime.Now,
                    BlockHashes = registryFullBlocks.Select(b => _defaultTransactionHashCalculation.CalculateHash(b?.RawData ?? new byte[Globals.DEFAULT_HASH_SIZE])).ToArray()
                };

                ISerializer combinedBlockSerializer = _serializersFactory.Create(synchronizationRegistryCombinedBlock);
                combinedBlockSerializer.SerializeBody();
                _nodeContext.SigningService.Sign(synchronizationRegistryCombinedBlock);
                combinedBlockSerializer.SerializeFully();

                IEnumerable<IKey> storageLayerKeys = _nodesResolutionService.GetStorageNodeKeys(combinedBlockSerializer);
                _communicationService.PostMessage(storageLayerKeys, combinedBlockSerializer);

                _synchronizationChainDataService.Add(synchronizationRegistryCombinedBlock);
            }
        }

        private void DistributeAndSaveFullBlock(IEnumerable<RegistryFullBlock> registryFullBlocks)
        {
            if (registryFullBlocks != null)
            {
                foreach (var registryFullBlock in registryFullBlocks)
                {
                    IRawPacketProvider fullBlockSerializer = _rawPacketProvidersFactory.Create(registryFullBlock);

                    IEnumerable<IKey> storageLayerKeys = _nodesResolutionService.GetStorageNodeKeys(fullBlockSerializer);
                    _communicationService.PostMessage(storageLayerKeys, fullBlockSerializer);

                    _registryChainDataService.Add(registryFullBlock);
                }
            }
        }

        #endregion Private Functions
    }
}
