using System.Collections.Generic;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.Node.Core.Registry
{
    //TODO: add performance counter for measuring MemPool size

    /// <summary>
    /// MemPool is needed for following purposes:
    ///  1. Source for building transactions registry block
    ///  2. Repository for comparing transactions registry key arrived with transactions registry block from another participant
    ///  
    ///  When created Transaction Registry Block gets approved by corresponding node from Sync layer transaction enumerated there must be removed from the Pool
    /// </summary>
    [RegisterDefaultImplementation(typeof(IRegistryMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistryMemPool : IRegistryMemPool
    {
        private readonly Dictionary<ulong, SortedDictionary<int, TransactionRegisterBlock>> _transactionRegisterBlocksOrdered;
        private readonly Dictionary<ulong, HashSet<TransactionRegisterBlock>> _transactionRegisterBlocks;
        private readonly Dictionary<ulong, int> _transactionsCounters;
        private readonly Dictionary<ulong, Dictionary<IKey, TransactionRegisterBlock>> _transactionRegistryByTransactionHash;
        private readonly Dictionary<ulong, Dictionary<byte, HashSet<TransactionsShortBlock>>> _transactionsShortBlocks;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private readonly ICryptoService _cryptoService;
        private readonly SynchronizationContext _synchronizationContext;
        private int _oldValue;
        private readonly object _sync = new object();

        public TransactionRegistryMemPool(ILoggerService loggerService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService, IStatesRepository statesRepository)
        {
            _oldValue = 0;
            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => 
            {
                _logger.Error($"MemPoolCount delta: {_transactionRegisterBlocksOrdered.Count - _oldValue}");
                _oldValue = _transactionRegisterBlocksOrdered.Count;
            };
            _timer.Start();

            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
            _logger = loggerService.GetLogger(nameof(TransactionRegistryMemPool));
            _transactionsCounters = new Dictionary<ulong, int>();
            _transactionRegisterBlocks = new Dictionary<ulong, HashSet<TransactionRegisterBlock>>();
            _transactionRegisterBlocksOrdered = new Dictionary<ulong, SortedDictionary<int, TransactionRegisterBlock>>();
            _transactionRegistryByTransactionHash = new Dictionary<ulong, Dictionary<IKey, TransactionRegisterBlock>>();
            _transactionsShortBlocks = new Dictionary<ulong, Dictionary<byte, HashSet<TransactionsShortBlock>>>();
            _cryptoService = cryptoService;
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
        }
        
        public bool EnqueueTransactionRegisterBlock(TransactionRegisterBlock transactionRegisterBlock)
        {
            lock(_sync)
            {
                if(!_transactionRegisterBlocks.ContainsKey(transactionRegisterBlock.SyncBlockHeight))
                {
                    _transactionsCounters.Add(transactionRegisterBlock.SyncBlockHeight, 0);
                    _transactionRegisterBlocks.Add(transactionRegisterBlock.SyncBlockHeight, new HashSet<TransactionRegisterBlock>());
                    _transactionRegisterBlocksOrdered.Add(transactionRegisterBlock.SyncBlockHeight, new SortedDictionary<int, TransactionRegisterBlock>());
                }

                if(_transactionRegisterBlocks[transactionRegisterBlock.SyncBlockHeight].Add(transactionRegisterBlock))
                {
                    _transactionRegisterBlocksOrdered[transactionRegisterBlock.SyncBlockHeight].Add(_transactionsCounters[transactionRegisterBlock.SyncBlockHeight]++, transactionRegisterBlock);

                    return true;
                }
            }

            return false;
        }

        public bool EnqueueTransactionsShortBlock(TransactionsShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                if(!_transactionsShortBlocks.ContainsKey(transactionsShortBlock.SyncBlockHeight))
                {
                    _transactionsShortBlocks.Add(transactionsShortBlock.SyncBlockHeight, new Dictionary<byte, HashSet<TransactionsShortBlock>>());
                }

                if(!_transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight].ContainsKey(transactionsShortBlock.Round))
                {
                    _transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight].Add(transactionsShortBlock.Round, new HashSet<TransactionsShortBlock>());
                }

                return _transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight][transactionsShortBlock.Round].Add(transactionsShortBlock);
            }
        }

        public int GetConfidenceRate(TransactionsShortBlock transactionsShortBlock) => throw new System.NotImplementedException();
        public void ClearByConfirmed(TransactionsShortBlock transactionsShortBlock) => throw new System.NotImplementedException();

        //TODO: need to understand whether it is needed to pass height of Sync Block or automatically take latest one?
        public IEnumerable<TransactionRegisterBlock> DequeueBulk(int maxCount)
        {
            List<TransactionRegisterBlock> items = new List<TransactionRegisterBlock>();
            lock(_sync)
            {
                ulong syncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;

                while (_transactionRegisterBlocksOrdered.Count > 0)
                {
                    foreach (int orderKey in _transactionRegisterBlocksOrdered[syncBlockHeight].Keys)
                    {
                        TransactionRegisterBlock transactionRegisterBlock = _transactionRegisterBlocksOrdered[syncBlockHeight][orderKey];
                    }
                }
            }

            return items;
        }
    }
}
