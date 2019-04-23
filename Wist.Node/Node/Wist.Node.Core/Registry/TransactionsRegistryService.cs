using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Serializers;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.PerformanceCounters;
using Wist.Core.Predicates;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.PerformanceCounters;
using Wist.Core.HashCalculations;
using Wist.Blockchain.Core;
using Wist.Core.Logging;
using Wist.Core.ExtensionMethods;
using System.Threading;
using Wist.Core;
using System.Threading.Tasks;
using Wist.Node.Core.Common;

namespace Wist.Node.Core.Registry
{
    [RegisterDefaultImplementation(typeof(ITransactionsRegistryService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryService : ITransactionsRegistryService
    {
        private readonly int _registrationPeriodMsec = 5000;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IPredicate _isBlockProducerPredicate;
        private readonly IRegistryMemPool _registryMemPool;
        private readonly IRegistryGroupState _registryGroupState;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly IServerCommunicationServicesRegistry _serverCommunicationServicesRegistry;
        private readonly ISerializersFactory _serializersFactory;
        private readonly INodeContext _nodeContext;
        private readonly IHashCalculation _powCalculation;
        private readonly IHashCalculation _hashCalculation;
        private IRegistryConfiguration _registryConfiguration;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _logger;
        private IServerCommunicationService _tcpCommunicationService;
        private IServerCommunicationService _udpCommunicationService;
        private readonly NodeCountersService _nodeCountersService;
        private IDisposable _syncContextUnsubscriber;
        private SyncCycleDescriptor _syncCycleDescriptor = null;

        public TransactionsRegistryService(IStatesRepository statesRepository, IPredicatesRepository predicatesRepository, IRegistryMemPool registryMemPool, 
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IConfigurationService configurationService, 
            IServerCommunicationServicesRegistry serverCommunicationServicesRegistry, IPerformanceCountersRepository performanceCountersRepository, 
            ISerializersFactory serializersFactory, IHashCalculationsRepository hashCalculationsRepository, ILoggerService loggerService)
        {
            _configurationService = configurationService;
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _registryGroupState = statesRepository.GetInstance<IRegistryGroupState>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _isBlockProducerPredicate = predicatesRepository.GetInstance("IsBlockProducer");
            _transactionHashKey = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _serverCommunicationServicesRegistry = serverCommunicationServicesRegistry;
            _serializersFactory = serializersFactory;
            _powCalculation = hashCalculationsRepository.Create(Globals.POW_TYPE);
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _logger = loggerService.GetLogger(nameof(TransactionsRegistryService));

            _registryMemPool = registryMemPool;

            _nodeCountersService = performanceCountersRepository.GetInstance<NodeCountersService>();
        }

        public void Initialize()
        {
            _registryConfiguration = _configurationService.Get<IRegistryConfiguration>();
            _tcpCommunicationService = _serverCommunicationServicesRegistry.GetInstance(_registryConfiguration.TcpServiceName);
            _udpCommunicationService = _serverCommunicationServicesRegistry.GetInstance(_registryConfiguration.UdpServiceName);
        }

        public void Start()
        {
            _syncContextUnsubscriber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)OnSyncContextChanged));
        }

        public void Stop()
        {
            _syncCycleDescriptor?.CancellationTokenSource?.Cancel();
            _syncContextUnsubscriber?.Dispose();
        }

        private void OnSyncContextChanged(string propName)
        {
            RecalculateProductionTimer();
        }

        private void RecalculateProductionTimer()
        {
            _registryGroupState.Round = 0;

            if(_syncCycleDescriptor != null)
            {
                _syncCycleDescriptor.CancellationTokenSource.Cancel();
                _syncCycleDescriptor.CancellationRequested = true;
            }

            _syncCycleDescriptor = new SyncCycleDescriptor(_synchronizationContext.LastBlockDescriptor);

            PeriodicTaskFactory.Start(o => 
            {
                SyncCycleDescriptor syncCycleDescriptor = (SyncCycleDescriptor)o;
                SortedList<ushort, RegistryRegisterBlock> transactionStateWitnesses = _registryMemPool.DequeueStateWitnessBulk();
                SortedList<ushort, RegistryRegisterUtxoConfidential> transactionUtxoWitnesses = _registryMemPool.DequeueUtxoWitnessBulk();

                RegistryFullBlock registryFullBlock = ProduceTransactionsFullBlock(transactionStateWitnesses, transactionUtxoWitnesses, syncCycleDescriptor.SynchronizationDescriptor, syncCycleDescriptor.Round);
                RegistryShortBlock registryShortBlock = ProduceTransactionsShortBlock(registryFullBlock);

                SendTransactionsBlocks(registryFullBlock, registryShortBlock);

                syncCycleDescriptor.Round++;

                if(syncCycleDescriptor.CancellationRequested)
                {
                    syncCycleDescriptor.CancellationTokenSource.Cancel();
                }
            }, _syncCycleDescriptor, _registrationPeriodMsec * _registryConfiguration.TotalNodes, _registryConfiguration.Position * _registrationPeriodMsec, cancelToken: _syncCycleDescriptor.CancellationTokenSource.Token, periodicTaskCreationOptions: TaskCreationOptions.LongRunning);
        }


        private RegistryFullBlock ProduceTransactionsFullBlock(SortedList<ushort, RegistryRegisterBlock> transactionStateWitnesses, SortedList<ushort, RegistryRegisterUtxoConfidential> transactionUtxoWitnesses, SynchronizationDescriptor synchronizationDescriptor, int round)
        {
            ulong syncBlockHeight = synchronizationDescriptor?.BlockHeight ?? 0;
            byte[] hash = synchronizationDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE];
            byte[] pow = _powCalculation.CalculateHash(hash);

            _logger.Debug($"ProduceTransactionsFullBlock synchronizationDescriptor[{syncBlockHeight}].Hash = {hash.ToHexString()}; POW = {pow.ToHexString()}");

            RegistryFullBlock transactionsFullBlock = new RegistryFullBlock
            {
                SyncBlockHeight = syncBlockHeight,
                PowHash = pow,
                BlockHeight = (ulong)(round * _registryConfiguration.TotalNodes + _registryConfiguration.Position + 1),
                StateWitnesses = transactionStateWitnesses.Select(t => t.Value).ToArray(),
                UtxoWitnesses = transactionUtxoWitnesses.Select(t => t.Value).ToArray()
            };

            return transactionsFullBlock;
        }

        private void SendTransactionsBlocks(RegistryFullBlock transactionsFullBlock, RegistryShortBlock transactionsShortBlock)
        {
            ISerializer fullBlockSerializer = _serializersFactory.Create(transactionsFullBlock);
            ISerializer shortBlockSerializer = _serializersFactory.Create(transactionsShortBlock);

            shortBlockSerializer.SerializeBody();
            _nodeContext.SigningService.Sign(transactionsShortBlock);

            shortBlockSerializer.SerializeFully();
            transactionsFullBlock.ShortBlockHash = _hashCalculation.CalculateHash(transactionsShortBlock.RawData);

            fullBlockSerializer.SerializeBody();
            _nodeContext.SigningService.Sign(transactionsFullBlock);

            _logger.Debug($"Sending FullBlock with {transactionsFullBlock.StateWitnesses.Length + transactionsFullBlock.UtxoWitnesses.Length} transactions and ShortBlock with {transactionsShortBlock.WitnessStateKeys.Length + transactionsShortBlock.WitnessUtxoKeys.Length} keys at round {transactionsFullBlock.BlockHeight}");

            _tcpCommunicationService.PostMessage(_registryGroupState.SyncLayerNode, fullBlockSerializer);
            _tcpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), shortBlockSerializer);
        }

        private RegistryShortBlock ProduceTransactionsShortBlock(RegistryFullBlock transactionsFullBlock)
        {
            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock
            {
                SyncBlockHeight = transactionsFullBlock.SyncBlockHeight,
                Nonce = transactionsFullBlock.Nonce,
                PowHash = transactionsFullBlock.PowHash,
                BlockHeight = transactionsFullBlock.BlockHeight,
                WitnessStateKeys = transactionsFullBlock.StateWitnesses.Select(w => new WitnessStateKey { PublicKey = w.Signer, Height = w.BlockHeight}).ToArray(),
                WitnessUtxoKeys = transactionsFullBlock.UtxoWitnesses.Select(w => new WitnessUtxoKey { KeyImage = w.KeyImage }).ToArray()
            };

            return transactionsShortBlock;
        }
    }
}
