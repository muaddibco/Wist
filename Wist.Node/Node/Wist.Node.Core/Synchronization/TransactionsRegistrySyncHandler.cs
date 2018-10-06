using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers;
using Wist.Network.Interfaces;
using Wist.Network.Topology;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Shards;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Serializers.RawPackets;
using Wist.Core.Logging;
using Wist.Core.ExtensionMethods;
using System.Runtime.CompilerServices;

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
        private readonly IHashCalculation _powCalculation;
        private readonly ISyncRegistryNeighborhoodState _syncRegistryNeighborhoodState;

        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly ISyncRegistryMemPool _syncRegistryMemPool;
        private readonly INodesResolutionService _nodesResolutionService;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IChainDataService _synchronizationChainDataService;
        private readonly IChainDataService _registryChainDataService;

        private readonly ILogger _logger;

        private IServerCommunicationService _communicationService;
        private CancellationToken _cancellationToken;

        public TransactionsRegistrySyncHandler(IStatesRepository statesRepository, ISyncShardsManager syncShardsManager, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            ICryptoService cryptoService, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, IHashCalculationsRepository hashCalculationsRepository, 
            IServerCommunicationServicesRegistry communicationServicesRegistry, ISyncRegistryMemPool syncRegistryMemPool, INodesResolutionService nodesResolutionService,
            IChainDataServicesManager chainDataServicesManager, IRawPacketProvidersFactory rawPacketProvidersFactory, ILoggerService loggerService)
        {
            _registryBlocks = new BlockingCollection<RegistryBlockBase>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _syncRegistryNeighborhoodState = statesRepository.GetInstance<ISyncRegistryNeighborhoodState>();
            _syncContextChangedUnsibsciber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)SynchronizationStateChanged));
            _syncShardsManager = syncShardsManager;
            _transactionHashKey = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _cryptoService = cryptoService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
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
            _communicationService = _communicationServicesRegistry.GetInstance("GenericTcp");

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            _logger.Debug($"{nameof(TransactionsRegistrySyncHandler)} - processing block {blockBase.RawData.ToHexString()}");
            RegistryBlockBase registryBlock = blockBase as RegistryBlockBase;

            if (registryBlock is RegistryFullBlock || registryBlock is RegistryConfidenceBlock)
            {
                _registryBlocks.Add(registryBlock);
            }
        }

        #region Private Functions

        private void SynchronizationStateChanged(string propName)
        {
            _votingCyclesCancellation.Cancel();
            _votingCyclesCancellation = new CancellationTokenSource();
            _votingCyclesStarted = false;
            if (CheckLeadershipParticipation())
            {
                LauchVotingSycles();
            }
        }

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (RegistryBlockBase registryBlock in _registryBlocks.GetConsumingEnumerable(ct))
            {
                try
                {
                    _logger.Debug($"{nameof(TransactionsRegistrySyncHandler)} - picked up for processing block {registryBlock.RawData.ToHexString()}");

                    lock (_syncRegistryMemPool)
                    {
                        if (registryBlock is RegistryFullBlock transactionsFullBlock)
                        {
                            if(transactionsFullBlock.TransactionHeaders.Count > 0)
                            {

                            }
                            _syncRegistryMemPool.AddCandidateBlock(transactionsFullBlock);
                        }
                        else if (registryBlock is RegistryConfidenceBlock transactionsRegistryConfidenceBlock)
                        {
                            if(transactionsRegistryConfidenceBlock.BitMask.Length > 0)
                            {

                            }
                            _syncRegistryMemPool.AddVotingBlock(transactionsRegistryConfidenceBlock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error during processing block", ex);
                }
            }
        }

        private CancellationTokenSource _votingCyclesCancellation = new CancellationTokenSource();
        private readonly AutoResetEvent _votingCyclesEntrySwitch = new AutoResetEvent(false);
        private bool _votingCyclesStarted;
        private readonly object _votingCyclesLaunchSync = new object();

        private bool CheckLeadershipParticipation()
        {
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LauchVotingSycles()
        {
            if (!_votingCyclesStarted)
            {
                lock (_votingCyclesLaunchSync)
                {
                    if (!_votingCyclesStarted)
                    {
                        _votingCyclesStarted = true;

                        Task.Factory.StartNew(() => DoVotingCycles(_votingCyclesCancellation.Token), _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                    }
                }
            }
        }

        private void DoVotingCycles(CancellationToken votingCycleCt)
        {
            Thread.Sleep(3000); // Shift voting cycles
            ulong round = 1;

            ulong syncHeight = _synchronizationContext.LastBlockDescriptor.BlockHeight;

            while (!_cancellationToken.IsCancellationRequested && !votingCycleCt.IsCancellationRequested)
            {
                // 1. check participation as leader 
                Thread.Sleep(5000);

                try
                {
                    RegistryFullBlock transactionsFullBlockMostConfident = null;

                    lock (_syncRegistryMemPool)
                    {
                        transactionsFullBlockMostConfident = _syncRegistryMemPool.GetMostConfidentFullBlock(round);
                        _syncRegistryMemPool.ResetRound(round++);
                    }

                    if (transactionsFullBlockMostConfident != null)
                    {
                        CreateAndDistributeConfirmationBlock(transactionsFullBlockMostConfident);

                        DistributeAndSaveFullBlock(transactionsFullBlockMostConfident);
                    }

                    CreateAndDistributeCombinedBlock(transactionsFullBlockMostConfident);

                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to complete Round due to error", ex);
                }
            }
            _syncRegistryMemPool.SetLastCompletedSyncHeight(syncHeight);
        }

        // every round must last around 5 seconds
        // when round ends following things will happen:
        // 1. Shard Leader will send block with information about Transactions Registry Block collected most votes
        // 2. Winning Transactions Registry Block will be saved to local database and will be sent to Storage Layer
        // 3. It will be created Combined Block that will hold information about winning Transactions Registry Block 
        private void RoundEndedHandler(RoundDescriptor roundDescriptor)
        {
            if (roundDescriptor == null)
            {
                return;
            }

        }

        private void CreateAndDistributeCombinedBlock(RegistryFullBlock transactionsFullBlockMostConfident)
        {
            SynchronizationRegistryCombinedBlock lastCombinedBlock = (SynchronizationRegistryCombinedBlock)_synchronizationChainDataService.GetAllLastBlocksByType(BlockTypes.Synchronization_RegistryCombinationBlock).Single();
            byte[] prevHash = lastCombinedBlock != null ? CryptoHelper.ComputeHash(lastCombinedBlock.BodyBytes) : new byte[Globals.DEFAULT_HASH_SIZE];
            byte[] fullBlockHash = CryptoHelper.ComputeHash(transactionsFullBlockMostConfident?.BodyBytes ?? new byte[Globals.DEFAULT_HASH_SIZE]);

            //TODO: For initial POC there will be only one participant at Synchronization Layer, thus combination of FullBlocks won't be implemented fully
            SynchronizationRegistryCombinedBlock synchronizationRegistryCombinedBlock = new SynchronizationRegistryCombinedBlock
            {
                SyncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0,
                PowHash = _powCalculation.CalculateHash(_synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE]),
                BlockHeight = _synchronizationContext.LastRegistrationCombinedBlockHeight + 1,
                HashPrev = prevHash,
                ReportedTime = DateTime.Now,
                BlockHashes = new byte[][] { fullBlockHash }
            };

            ISignatureSupportSerializer combinedBlockSerializer = _signatureSupportSerializersFactory.Create(synchronizationRegistryCombinedBlock);
            combinedBlockSerializer.FillBodyAndRowBytes();

            IEnumerable<IKey> storageLayerKeys = _nodesResolutionService.GetStorageNodeKeys(combinedBlockSerializer);
            _communicationService.PostMessage(storageLayerKeys, combinedBlockSerializer);

            _synchronizationChainDataService.Add(synchronizationRegistryCombinedBlock);

            _synchronizationContext.LastRegistrationCombinedBlockHeight++;
        }

        private void DistributeAndSaveFullBlock(RegistryFullBlock transactionsFullBlockMostConfident)
        {
            IRawPacketProvider fullBlockSerializer = _rawPacketProvidersFactory.Create(transactionsFullBlockMostConfident);

            IEnumerable<IKey> storageLayerKeys = _nodesResolutionService.GetStorageNodeKeys(fullBlockSerializer);
            _communicationService.PostMessage(storageLayerKeys, fullBlockSerializer);

            _registryChainDataService.Add(transactionsFullBlockMostConfident);
        }

        private void CreateAndDistributeConfirmationBlock(RegistryFullBlock transactionsFullBlockMostConfident)
        {
            RegistryConfirmationBlock registryConfirmationBlock = new RegistryConfirmationBlock
            {
                SyncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0,
                PowHash = _powCalculation.CalculateHash(_synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE]),
                BlockHeight = transactionsFullBlockMostConfident.BlockHeight,
                ReferencedBlockHash = transactionsFullBlockMostConfident.ShortBlockHash
            };

            ShardDescriptor shardDescriptor = _syncShardsManager.GetShardDescriptorByRound((int)transactionsFullBlockMostConfident.BlockHeight);
            ISignatureSupportSerializer registryConfirmationBlockSerializer = _signatureSupportSerializersFactory.Create(registryConfirmationBlock);

            _communicationService.PostMessage(_syncRegistryNeighborhoodState.GetAllNeighbors(), registryConfirmationBlockSerializer);
        }

        #endregion Private Functions
    }
}
