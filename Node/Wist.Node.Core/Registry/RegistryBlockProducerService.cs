using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
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
        private readonly IRegistryGroupState _registryGroupState;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ICryptoService _cryptoService;
        private Timer _timer;
        private IDisposable _syncContextUnsubscriber;

        public RegistryBlockProducerService(IStatesRepository statesRepository, IPredicatesRepository predicatesRepository, IRegistryMemPool registryMemPool, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService)
        {
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
            _registryGroupState = statesRepository.GetInstance<RegistryGroupState>();
            _isBlockProducerPredicate = predicatesRepository.GetInstance("IsBlockProducer");
            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
            _cryptoService = cryptoService;

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
                TransactionHeaderHashes = new SortedList<ushort, IKey>(transactionsFullBlock.TransactionHeaders.ToDictionary(i => i.Key, i => GetTransactionRegistryHashKey(i.Value)))
            };

            return transactionsShortBlock;
        }

        private void SendTransactionsShortBlock(TransactionsShortBlock transactionsShortBlock)
        {

        }

        private IKey GetTransactionRegistryHashKey(TransactionRegisterBlock transactionRegisterBlock)
        {
            byte[] transactionHeightBytes = BitConverter.GetBytes(transactionRegisterBlock.BlockHeight);

            byte[] senderAndHeightBytes = new byte[transactionRegisterBlock.Key.Length + transactionHeightBytes.Length];

            Array.Copy(transactionRegisterBlock.Key.Value, senderAndHeightBytes, transactionRegisterBlock.Key.Length);
            Array.Copy(transactionHeightBytes, 0, senderAndHeightBytes, transactionRegisterBlock.Key.Length, transactionHeightBytes.Length);

            IKey key = _transactionHashKey.GetKey(_cryptoService.ComputeTransactionKey(senderAndHeightBytes));

            return key;
        }
    }
}
