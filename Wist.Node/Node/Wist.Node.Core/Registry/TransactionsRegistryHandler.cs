using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.Core.Serializers.RawPackets;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;
using Wist.Core.Configuration;
using Wist.Core.HashCalculations;
using Wist.Core.States;
using Wist.Node.Core.Common;
using Wist.Blockchain.Core.Serializers;
using Wist.Core.Synchronization;
using Wist.Core.Logging;
using Wist.Core.ExtensionMethods;
using Wist.Core.Models;
using Wist.Core;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistry";

        private readonly ITargetBlock<RegistryShortBlock> _processWitnessedFlow;
        private readonly BlockingCollection<RegistryRegisterBlock> _registryStateBlocks;
        private readonly BlockingCollection<RegistryRegisterUtxoConfidential> _registryUtxoBlocks;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IRegistryMemPool _registryMemPool;
        private readonly IConfigurationService _configurationService;
        private readonly IRegistryGroupState _registryGroupState;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IHashCalculation _defaulHashCalculation;
        private readonly IHashCalculation _powCalculation;
        private readonly INodeContext _nodeContext;
        private readonly ILogger _logger;
        private IServerCommunicationService _udpCommunicationService;
        private IServerCommunicationService _tcpCommunicationService;

        public TransactionsRegistryHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, 
            IRawPacketProvidersFactory rawPacketProvidersFactory, IRegistryMemPool registryMemPool, IConfigurationService configurationService, 
            IHashCalculationsRepository hashCalculationRepository, ILoggerService loggerService)
        {
            _registryStateBlocks = new BlockingCollection<RegistryRegisterBlock>();
            _registryUtxoBlocks = new BlockingCollection<RegistryRegisterUtxoConfidential>();
            _registryGroupState = statesRepository.GetInstance<IRegistryGroupState>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _registryMemPool = registryMemPool;
            _configurationService = configurationService;
            _defaulHashCalculation = hashCalculationRepository.Create(Globals.DEFAULT_HASH);
            _powCalculation = hashCalculationRepository.Create(Globals.POW_TYPE);
            _logger = loggerService.GetLogger(nameof(TransactionsRegistryHandler));

            _processWitnessedFlow = new ActionBlock<RegistryShortBlock>((Action<RegistryShortBlock>)ProcessWitnessed);
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _udpCommunicationService = _communicationServicesRegistry.GetInstance(_configurationService.Get<IRegistryConfiguration>().UdpServiceName);
            _tcpCommunicationService = _communicationServicesRegistry.GetInstance(_configurationService.Get<IRegistryConfiguration>().TcpServiceName);

            Task.Factory.StartNew(() => {
                ProcessStateBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            Task.Factory.StartNew(() => {
                ProcessUtxoBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void ProcessBlock(PacketBase blockBase)
        {

            if (blockBase is RegistryRegisterBlock transactionRegisterStateBlock)
            {
                _registryStateBlocks.Add(transactionRegisterStateBlock);
            }

            if (blockBase is RegistryRegisterUtxoConfidential transactionRegisterUtxoBlock)
            {
                _registryUtxoBlocks.Add(transactionRegisterUtxoBlock);
            }

            if (blockBase is RegistryShortBlock transactionsShortBlock)
            {
                _processWitnessedFlow.Post(transactionsShortBlock);
            }
        }

        #region Private Functions

        private void ProcessStateBlocks(CancellationToken ct)
        {
            foreach (RegistryRegisterBlock transactionRegisterBlock in _registryStateBlocks.GetConsumingEnumerable(ct))
            {
                //TODO: add logic that will check whether received Transaction Header was already stored into blockchain

                bool isNew = _registryMemPool.EnqueueTransactionWitness(transactionRegisterBlock);

                if (isNew)
                {
                    IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(transactionRegisterBlock);
                    _udpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), packetProvider);
                }
            }
        }

        private void ProcessUtxoBlocks(CancellationToken ct)
        {
            foreach (RegistryRegisterUtxoConfidential transactionRegisterUtxo in _registryUtxoBlocks.GetConsumingEnumerable(ct))
            {
                //TODO: add logic that will check whether received Transaction Header was already stored into blockchain

                bool isNew = _registryMemPool.EnqueueTransactionWitness(transactionRegisterUtxo);

                if (isNew)
                {
                    IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(transactionRegisterUtxo);
                    _udpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), packetProvider);
                }
            }
        }

        private void ProcessWitnessed(RegistryShortBlock registryShortBlock)
        {
            _registryGroupState.ToggleLastBlockConfirmationReceived();

            if (registryShortBlock != null)
            {
                _registryMemPool.ClearWitnessed(registryShortBlock);
            }

            //TODO: obtain Transactions Registry Short block from MemPool by hash given in confirmationBlock
            //TODO: clear MemPool from Transaction Headers of confirmed Short Block
        }

        #endregion PrivateFunctions
    }
}
