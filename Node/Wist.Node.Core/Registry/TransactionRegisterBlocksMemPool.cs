using System.Collections.Concurrent;
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
    public class TransactionRegisterBlocksMemPool : MemPoolBase<TransactionRegisterBlock>
    {
        private readonly ConcurrentDictionary<int, TransactionRegisterBlock> _transactionRegisterBlocks;
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
            _transactionRegisterBlocks = new ConcurrentDictionary<int, TransactionRegisterBlock>();
        }

        public override bool AddIfNotExist(TransactionRegisterBlock item)
        {
            TransactionRegisterBlock transactionRegisterBlock = _transactionRegisterBlocks.AddOrUpdate(item.GetHashCode(), item, (k, v) => v);

            return transactionRegisterBlock == item;
        }
    }
}
