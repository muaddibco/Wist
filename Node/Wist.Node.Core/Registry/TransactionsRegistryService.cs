using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Serializers;
using Wist.Communication.Interfaces;
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

namespace Wist.Node.Core.Registry
{
    [RegisterDefaultImplementation(typeof(ITransactionsRegistryService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryService : ITransactionsRegistryService
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IPredicate _isBlockProducerPredicate;
        private readonly ITargetBlock<IRegistryMemPool> _transactionsRegistryProducingFlow;
        private readonly IRegistryMemPool _registryMemPool;
        private readonly IRegistryGroupState _registryGroupState;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ICryptoService _cryptoService;
        private readonly IConfigurationService _configurationService;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly IServerCommunicationService _tcpCommunicationService;
        private readonly IServerCommunicationService _udpCommunicationService;
        private readonly NodeCountersService _nodeCountersService;
        private Timer _timer;
        private IDisposable _syncContextUnsubscriber;

        public TransactionsRegistryService(IStatesRepository statesRepository, IPredicatesRepository predicatesRepository, IRegistryMemPool registryMemPool, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService, IConfigurationService configurationService, IServerCommunicationServicesRegistry serverCommunicationServicesRegistry, IPerformanceCountersRepository performanceCountersRepository, ISignatureSupportSerializersFactory signatureSupportSerializersFactory)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _registryGroupState = statesRepository.GetInstance<IRegistryGroupState>();
            _isBlockProducerPredicate = predicatesRepository.GetInstance("IsBlockProducer");
            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
            _cryptoService = cryptoService;
            _configurationService = configurationService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            TransformBlock<IRegistryMemPool, SortedList<ushort, RegistryRegisterBlock>> deduplicateAndOrderTransactionRegisterBlocksBlock = new TransformBlock<IRegistryMemPool, SortedList<ushort, RegistryRegisterBlock>>((Func<IRegistryMemPool, SortedList<ushort, RegistryRegisterBlock>>)DeduplicateAndOrderTransactionRegisterBlocks);
            TransformBlock<SortedList<ushort, RegistryRegisterBlock>, RegistryFullBlock> produceTransactionsFullBlock = new TransformBlock<SortedList<ushort, RegistryRegisterBlock>, RegistryFullBlock>((Func<SortedList<ushort, RegistryRegisterBlock>, RegistryFullBlock>)ProduceTransactionsFullBlock);
            ActionBlock<Tuple<RegistryFullBlock, RegistryShortBlock>> sendTransactionsFullBlock = new ActionBlock<Tuple<RegistryFullBlock, RegistryShortBlock>>((Action<Tuple<RegistryFullBlock, RegistryShortBlock>>)SendTransactionsFullBlock);
            TransformBlock<RegistryFullBlock, Tuple<RegistryFullBlock, RegistryShortBlock>> produceTransactionsShortBlock = new TransformBlock<RegistryFullBlock, Tuple<RegistryFullBlock, RegistryShortBlock>>((Func<RegistryFullBlock, Tuple<RegistryFullBlock, RegistryShortBlock>>)ProduceTransactionsShortBlock);
            ActionBlock<Tuple<RegistryFullBlock, RegistryShortBlock>> sendTransactionsShortBlock = new ActionBlock<Tuple<RegistryFullBlock, RegistryShortBlock>>((Action<Tuple<RegistryFullBlock, RegistryShortBlock>>)SendTransactionsShortBlock);

            deduplicateAndOrderTransactionRegisterBlocksBlock.LinkTo(produceTransactionsFullBlock);
            produceTransactionsFullBlock.LinkTo(produceTransactionsShortBlock);
            produceTransactionsShortBlock.LinkTo(sendTransactionsFullBlock);
            produceTransactionsShortBlock.LinkTo(sendTransactionsShortBlock);

            _transactionsRegistryProducingFlow = deduplicateAndOrderTransactionRegisterBlocksBlock;


            _registryMemPool = registryMemPool;
            _tcpCommunicationService = serverCommunicationServicesRegistry.GetInstance(_configurationService.Get<IRegistryConfiguration>().TcpServiceName);
            _udpCommunicationService = serverCommunicationServicesRegistry.GetInstance(_configurationService.Get<IRegistryConfiguration>().UdpServiceName);

            _nodeCountersService = performanceCountersRepository.GetInstance<NodeCountersService>();
        }

        public void Initialize()
        {
        }

        public void Start()
        {
            _syncContextUnsubscriber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)OnSyncContextChanged));
        }

        public void Stop()
        {
            StopTimer();
            _syncContextUnsubscriber?.Dispose();
        }

        private void OnSyncContextChanged(string propName)
        {
            RecalculateProductionTimer();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _registryGroupState.Round++;

            if (_isBlockProducerPredicate.Evaluate())
            {
                _registryGroupState.WaitLastBlockConfirmationReceived();
                _transactionsRegistryProducingFlow.Post(_registryMemPool);
            }
        }

        private void RecalculateProductionTimer()
        {
            StopTimer();

            _registryGroupState.Round = 0;
            _registryGroupState.ToggleLastBlockConfirmationReceived();

            _timer = new Timer(5000);
            _timer.Elapsed += _timer_Elapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer?.Stop();
            _timer?.Dispose();
        }

        private SortedList<ushort, RegistryRegisterBlock> DeduplicateAndOrderTransactionRegisterBlocks(IRegistryMemPool memPool)
        {
            SortedList<ushort, RegistryRegisterBlock> transactionRegisterBlocks = memPool.DequeueBulk(-1);

            return transactionRegisterBlocks;
        }

        private RegistryFullBlock ProduceTransactionsFullBlock(SortedList<ushort, RegistryRegisterBlock> transactionRegisterBlocks)
        {
            RegistryFullBlock transactionsFullBlock = new RegistryFullBlock
            {
                SyncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight??0,
                BlockHeight = (ulong)_registryGroupState.Round,
                TransactionHeaders = transactionRegisterBlocks
            };

            _nodeCountersService.RegistryBlockLastSize.RawValue = transactionRegisterBlocks.Count;
            _nodeCountersService.RegistryBlockLastSize.NextSample();

            return transactionsFullBlock;
        }

        private void SendTransactionsFullBlock(Tuple<RegistryFullBlock, RegistryShortBlock> tuple)
        {
            RegistryFullBlock transactionsFullBlock = tuple.Item1;
            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionsFullBlock);
            _tcpCommunicationService.PostMessage(_registryGroupState.SyncLayerNode, signatureSupportSerializer);
        }

        private Tuple<RegistryFullBlock, RegistryShortBlock> ProduceTransactionsShortBlock(RegistryFullBlock transactionsFullBlock)
        {
            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock
            {
                SyncBlockHeight = transactionsFullBlock.SyncBlockHeight,
                BlockHeight = transactionsFullBlock.BlockHeight,
                TransactionHeaderHashes = new SortedList<ushort, IKey>(transactionsFullBlock.TransactionHeaders.ToDictionary(i => i.Key, i => i.Value.GetTransactionRegistryHashKey(_cryptoService, _transactionHashKey)))
            };
            Tuple<RegistryFullBlock, RegistryShortBlock> tuple = new Tuple<RegistryFullBlock, RegistryShortBlock>(transactionsFullBlock, transactionsShortBlock);

            return tuple;
        }

        private void SendTransactionsShortBlock(Tuple<RegistryFullBlock, RegistryShortBlock> tuple)
        {
            RegistryShortBlock transactionsShortBlock = tuple.Item2;
            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionsShortBlock);
            _tcpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), signatureSupportSerializer);
        }
    }
}
