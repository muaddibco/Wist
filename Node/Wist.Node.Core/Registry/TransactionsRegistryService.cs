using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
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
        private readonly IServerCommunicationService _tcpCommunicationService;
        private readonly IServerCommunicationService _udpCommunicationService;
        private readonly NodeCountersService _nodeCountersService;
        private Timer _timer;
        private IDisposable _syncContextUnsubscriber;

        public TransactionsRegistryService(IStatesRepository statesRepository, IPredicatesRepository predicatesRepository, IRegistryMemPool registryMemPool, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService, IConfigurationService configurationService, IServerCommunicationServicesRegistry serverCommunicationServicesRegistry, IPerformanceCountersRepository performanceCountersRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
            _registryGroupState = statesRepository.GetInstance<RegistryGroupState>();
            _isBlockProducerPredicate = predicatesRepository.GetInstance("IsBlockProducer");
            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
            _cryptoService = cryptoService;
            _configurationService = configurationService;

            TransformBlock<IRegistryMemPool, SortedList<ushort, TransactionRegisterBlock>> deduplicateAndOrderTransactionRegisterBlocksBlock = new TransformBlock<IRegistryMemPool, SortedList<ushort, TransactionRegisterBlock>>((Func<IRegistryMemPool, SortedList<ushort, TransactionRegisterBlock>>)DeduplicateAndOrderTransactionRegisterBlocks);
            TransformBlock<SortedList<ushort, TransactionRegisterBlock>, TransactionsFullBlock> produceTransactionsFullBlock = new TransformBlock<SortedList<ushort, TransactionRegisterBlock>, TransactionsFullBlock>((Func<SortedList<ushort, TransactionRegisterBlock>, TransactionsFullBlock>)ProduceTransactionsFullBlock);
            ActionBlock<TransactionsFullBlock> sendTransactionsFullBlock = new ActionBlock<TransactionsFullBlock>((Action<TransactionsFullBlock>)SendTransactionsFullBlock);
            TransformBlock<TransactionsFullBlock, TransactionsShortBlock> produceTransactionsShortBlock = new TransformBlock<TransactionsFullBlock, TransactionsShortBlock>((Func<TransactionsFullBlock, TransactionsShortBlock>)ProduceTransactionsShortBlock);
            ActionBlock<TransactionsShortBlock> sendTransactionsShortBlock = new ActionBlock<TransactionsShortBlock>((Action<TransactionsShortBlock>)SendTransactionsShortBlock);

            deduplicateAndOrderTransactionRegisterBlocksBlock.LinkTo(produceTransactionsFullBlock);
            produceTransactionsFullBlock.LinkTo(sendTransactionsFullBlock);
            produceTransactionsFullBlock.LinkTo(produceTransactionsShortBlock);
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

        private SortedList<ushort, TransactionRegisterBlock> DeduplicateAndOrderTransactionRegisterBlocks(IRegistryMemPool memPool)
        {
            SortedList<ushort, TransactionRegisterBlock> transactionRegisterBlocks = memPool.DequeueBulk(-1);

            return transactionRegisterBlocks;
        }

        private TransactionsFullBlock ProduceTransactionsFullBlock(SortedList<ushort, TransactionRegisterBlock> transactionRegisterBlocks)
        {
            TransactionsFullBlock transactionsFullBlock = new TransactionsFullBlock
            {
                Round = (byte)_registryGroupState.Round,
                TransactionHeaders = transactionRegisterBlocks
            };

            _nodeCountersService.RegistryBlockLastSize.RawValue = transactionRegisterBlocks.Count;
            _nodeCountersService.RegistryBlockLastSize.NextSample();

            return transactionsFullBlock;
        }

        private void SendTransactionsFullBlock(TransactionsFullBlock transactionsFullBlock)
        {
        }

        private TransactionsShortBlock ProduceTransactionsShortBlock(TransactionsFullBlock transactionsFullBlock)
        {
            TransactionsShortBlock transactionsShortBlock = new TransactionsShortBlock
            {
                Round = (byte)_registryGroupState.Round,
                TransactionHeaderHashes = new SortedList<ushort, IKey>(transactionsFullBlock.TransactionHeaders.ToDictionary(i => i.Key, i => i.Value.GetTransactionRegistryHashKey(_cryptoService, _transactionHashKey)))
            };

            return transactionsShortBlock;
        }

        private void SendTransactionsShortBlock(TransactionsShortBlock transactionsShortBlock)
        {

        }
    }
}
