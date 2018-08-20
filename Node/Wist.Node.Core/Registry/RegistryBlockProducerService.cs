using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Predicates;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Registry
{
    [RegisterDefaultImplementation(typeof(IRegistryBlockProducerService), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryBlockProducerService : IRegistryBlockProducerService
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IPredicate _isBlockProducerPredicate;
        private readonly ITargetBlock<IRegistryMemPool> _transactionsRegistryProducingFlow;
        private readonly IRegistryMemPool _registryMemPool;
        private Timer _timer;
        private IDisposable _syncContextUnsubscriber;

        public RegistryBlockProducerService(IStatesRepository statesRepository, IPredicatesRepository predicatesRepository, IRegistryMemPool registryMemPool)
        {
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
            _isBlockProducerPredicate = predicatesRepository.GetInstance("IsBlockProducer");

            TransformBlock<IRegistryMemPool, SortedList<int, TransactionHeader>> deduplicateAndOrderTransactionHeadersBlock = new TransformBlock<IRegistryMemPool, SortedList<int, TransactionHeader>>((Func<IRegistryMemPool, SortedList<int, TransactionHeader>>)DeduplicateAndOrderTransactionHeaders);
            TransformBlock<SortedList<int, TransactionHeader>, TransactionsFullBlock> produceTransactionsFullBlock = new TransformBlock<SortedList<int, TransactionHeader>, TransactionsFullBlock>((Func<SortedList<int, TransactionHeader>, TransactionsFullBlock>)ProduceTransactionsFullBlock);
            ActionBlock<TransactionsFullBlock> sendTransactionsFullBlock = new ActionBlock<TransactionsFullBlock>((Action<TransactionsFullBlock>)SendTransactionsFullBlock);
            TransformBlock<TransactionsFullBlock, TransactionsShortBlock> produceTransactionsShortBlock = new TransformBlock<TransactionsFullBlock, TransactionsShortBlock>((Func<TransactionsFullBlock, TransactionsShortBlock>)ProduceTransactionsShortBlock);
            ActionBlock<TransactionsShortBlock> sendTransactionsShortBlock = new ActionBlock<TransactionsShortBlock>((Action<TransactionsShortBlock>)SendTransactionsShortBlock);

            deduplicateAndOrderTransactionHeadersBlock.LinkTo(produceTransactionsFullBlock);
            produceTransactionsFullBlock.LinkTo(sendTransactionsFullBlock);
            produceTransactionsFullBlock.LinkTo(produceTransactionsShortBlock);
            produceTransactionsShortBlock.LinkTo(sendTransactionsShortBlock);

            _transactionsRegistryProducingFlow = deduplicateAndOrderTransactionHeadersBlock;
            _registryMemPool = registryMemPool;
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
            if(_isBlockProducerPredicate.Evaluate())
            {
                _transactionsRegistryProducingFlow.Post(_registryMemPool);
            }
        }

        private void RecalculateProductionTimer()
        {
            StopTimer();

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

        private SortedList<int, TransactionHeader> DeduplicateAndOrderTransactionHeaders(IRegistryMemPool memPool)
        {
            SortedList<int, TransactionHeader> deduplicatedOrderedTransactionHeaders = new SortedList<int, TransactionHeader>();

            return deduplicatedOrderedTransactionHeaders;
        }

        private TransactionsFullBlock ProduceTransactionsFullBlock(SortedList<int, TransactionHeader> transactionHeaders)
        {
            TransactionsFullBlock transactionsFullBlock = null;

            return transactionsFullBlock;
        }

        private void SendTransactionsFullBlock(TransactionsFullBlock transactionsFullBlock)
        {
        }

        private TransactionsShortBlock ProduceTransactionsShortBlock(TransactionsFullBlock transactionsFullBlock)
        {
            TransactionsShortBlock transactionsShortBlock = new TransactionsShortBlock();

            return transactionsShortBlock;
        }

        private void SendTransactionsShortBlock(TransactionsShortBlock transactionsShortBlock)
        {

        }
    }
}
