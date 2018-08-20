using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
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
        private readonly ConcurrentDictionary<IKey, TransactionRegisterBlock> _transactionRegistryByKey;
        private readonly ConcurrentQueue<TransactionRegisterBlock> _transactionRegisterBlocksQueue;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private readonly ICryptoService _cryptoService;
        private int _oldValue;

        public TransactionRegisterBlocksMemPool(ILoggerService loggerService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService)
        {
            _oldValue = 0;
            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => 
            {
                _logger.Error($"MemPoolCount delta: {_transactionRegisterBlocks.Count - _oldValue}");
                _oldValue = _transactionRegisterBlocks.Count;
            };
            _timer.Start();

            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
            _logger = loggerService.GetLogger(nameof(TransactionRegisterBlocksMemPool));
            _transactionRegisterBlocks = new ConcurrentDictionary<int, TransactionRegisterBlock>();
            _transactionRegisterBlocksQueue = new ConcurrentQueue<TransactionRegisterBlock>();
            _transactionRegistryByKey = new ConcurrentDictionary<IKey, TransactionRegisterBlock>(_identityKeyProvider.GetComparer());
            _cryptoService = cryptoService;
        }

        public override bool Enqueue(TransactionRegisterBlock item)
        {
            TransactionRegisterBlock transactionRegisterBlock = _transactionRegisterBlocks.AddOrUpdate(item.GetHashCode(), item, (k, v) => v);

            if(transactionRegisterBlock == item)
            {
                _transactionRegisterBlocksQueue.Enqueue(item);
                byte[] transactionKey = _cryptoService.ComputeTransactionKey(item.BodyBytes);
                _transactionRegistryByKey.AddOrUpdate(_identityKeyProvider.GetKey(transactionKey), item, (k, v) => v);

                return true;
            }

            return false;
        }
        
        public override IEnumerable<TransactionRegisterBlock> DequeueBulk(int maxCount)
        {
            int collectedCount;

            List<TransactionRegisterBlock> transactionHeaders = new List<TransactionRegisterBlock>();

            do
            {
                TransactionRegisterBlock transactionHeader;

                if(!_transactionRegisterBlocksQueue.TryDequeue(out transactionHeader))
                {
                    break;
                }


            } while (true);

            return transactionHeaders;
        }

        public override void RemoveAll(IEnumerable<TransactionRegisterBlock> items)
        {
            throw new System.NotImplementedException();
        }
    }
}
