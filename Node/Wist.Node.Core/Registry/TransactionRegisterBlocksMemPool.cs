using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.MemPools;

namespace Wist.Node.Core.Registry
{
    //TODO: add performance counter for measuring MemPool size
    [RegisterExtension(typeof(IMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegisterBlocksMemPool : MemPoolBase<KeyedTransactionHeader>
    {
        private readonly ConcurrentDictionary<int, KeyedTransactionHeader> _transactionRegisterBlocks;
        private readonly ConcurrentQueue<KeyedTransactionHeader> _transactionRegisterBlocksQueue;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private int _oldValue;

        public TransactionRegisterBlocksMemPool(ILoggerService loggerService)
        {
            _oldValue = 0;
            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => 
            {
                _logger.Error($"MemPoolCount delta: {_transactionRegisterBlocks.Count - _oldValue}");
                _oldValue = _transactionRegisterBlocks.Count;
            };
            _timer.Start();
            _logger = loggerService.GetLogger(nameof(TransactionRegisterBlocksMemPool));
            _transactionRegisterBlocks = new ConcurrentDictionary<int, KeyedTransactionHeader>();
            _transactionRegisterBlocksQueue = new ConcurrentQueue<KeyedTransactionHeader>();
        }

        public override bool Enqueue(KeyedTransactionHeader item)
        {
            KeyedTransactionHeader transactionRegisterBlock = _transactionRegisterBlocks.AddOrUpdate(item.GetHashCode(), item, (k, v) => v);

            if(transactionRegisterBlock == item)
            {
                _transactionRegisterBlocksQueue.Enqueue(item);

                return true;
            }

            return false;
        }
        
        public override IEnumerable<KeyedTransactionHeader> DequeueBulk(int maxCount)
        {
            int collectedCount;

            List<KeyedTransactionHeader> transactionHeaders = new List<KeyedTransactionHeader>();

            do
            {
                KeyedTransactionHeader transactionHeader;

                if(!_transactionRegisterBlocksQueue.TryDequeue(out transactionHeader))
                {
                    break;
                }


            } while (true);

            return transactionHeaders;
        }

        public override void RemoveAll(IEnumerable<KeyedTransactionHeader> items)
        {
            throw new System.NotImplementedException();
        }
    }
}
