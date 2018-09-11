using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Dictionary<ulong, SortedDictionary<int, RegistryRegisterBlock>> _transactionRegisterBlocksOrdered;
        private readonly Dictionary<ulong, int> _transactionsCounters;

        // Key of this dictionary is hash of concatenation of Public Key of sender and Height of transaction
        private readonly Dictionary<ulong, Dictionary<IKey, RegistryRegisterBlock>> _transactionRegistryByTransactionHash;
        private readonly Dictionary<ulong, Dictionary<IKey, int>> _transactionOrderByTransactionHash;

        private readonly Dictionary<ulong, Dictionary<ulong, HashSet<RegistryShortBlock>>> _transactionsShortBlocks;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private readonly ISynchronizationContext _synchronizationContext;
        private int _oldValue;
        private readonly object _sync = new object();

        public TransactionRegistryMemPool(ILoggerService loggerService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService, IStatesRepository statesRepository)
        {
            _oldValue = 0;
            _timer = new Timer(1000);
            _timer.Elapsed += (s, e) => 
            {
                if (_synchronizationContext.LastBlockDescriptor != null && _transactionRegisterBlocksOrdered.ContainsKey(_synchronizationContext.LastBlockDescriptor.BlockHeight))
                {
                    _logger.Error($"MemPoolCount delta: {_transactionRegisterBlocksOrdered[_synchronizationContext.LastBlockDescriptor.BlockHeight].Count - _oldValue}");
                    _oldValue = _transactionRegisterBlocksOrdered[_synchronizationContext.LastBlockDescriptor.BlockHeight].Count;
                }
            };
            _timer.Start();

            _transactionHashKey = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _logger = loggerService.GetLogger(nameof(TransactionRegistryMemPool));
            _transactionsCounters = new Dictionary<ulong, int>();
            _transactionRegisterBlocksOrdered = new Dictionary<ulong, SortedDictionary<int, RegistryRegisterBlock>>();
            _transactionRegistryByTransactionHash = new Dictionary<ulong, Dictionary<IKey, RegistryRegisterBlock>>();
            _transactionsShortBlocks = new Dictionary<ulong, Dictionary<ulong, HashSet<RegistryShortBlock>>>();
            _transactionOrderByTransactionHash = new Dictionary<ulong, Dictionary<IKey, int>>();
            _cryptoService = cryptoService;
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
        }
        
        public bool EnqueueTransactionRegisterBlock(RegistryRegisterBlock transactionRegisterBlock)
        {
            lock(_sync)
            {
                if(!_transactionRegistryByTransactionHash.ContainsKey(transactionRegisterBlock.SyncBlockHeight))
                {
                    _transactionsCounters.Add(transactionRegisterBlock.SyncBlockHeight, 0);
                    _transactionRegisterBlocksOrdered.Add(transactionRegisterBlock.SyncBlockHeight, new SortedDictionary<int, RegistryRegisterBlock>());
                    _transactionRegistryByTransactionHash.Add(transactionRegisterBlock.SyncBlockHeight, new Dictionary<IKey, RegistryRegisterBlock>());
                    _transactionOrderByTransactionHash.Add(transactionRegisterBlock.SyncBlockHeight, new Dictionary<IKey, int>());
                }

                IKey key = transactionRegisterBlock.GetTransactionRegistryHashKey(_cryptoService, _transactionHashKey);

                if (!_transactionRegistryByTransactionHash[transactionRegisterBlock.SyncBlockHeight].ContainsKey(key))
                {
                    _transactionRegisterBlocksOrdered[transactionRegisterBlock.SyncBlockHeight].Add(_transactionsCounters[transactionRegisterBlock.SyncBlockHeight], transactionRegisterBlock);

                    _transactionRegistryByTransactionHash[transactionRegisterBlock.SyncBlockHeight].Add(key, transactionRegisterBlock);
                    _transactionOrderByTransactionHash[transactionRegisterBlock.SyncBlockHeight].Add(key, _transactionsCounters[transactionRegisterBlock.SyncBlockHeight]);
                    _transactionsCounters[transactionRegisterBlock.SyncBlockHeight]++;
                    return true;
                }
            }

            return false;
        }

        public bool EnqueueTransactionsShortBlock(RegistryShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                if(!_transactionsShortBlocks.ContainsKey(transactionsShortBlock.SyncBlockHeight))
                {
                    _transactionsShortBlocks.Add(transactionsShortBlock.SyncBlockHeight, new Dictionary<ulong, HashSet<RegistryShortBlock>>());
                }

                if(!_transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight].ContainsKey(transactionsShortBlock.BlockHeight))
                {
                    _transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight].Add(transactionsShortBlock.BlockHeight, new HashSet<RegistryShortBlock>());
                }

                return _transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight][transactionsShortBlock.BlockHeight].Add(transactionsShortBlock);
            }
        }

        public int GetConfidenceRate(RegistryShortBlock transactionsShortBlock)
        {
            lock(_sync)
            {
                Dictionary<IKey, RegistryRegisterBlock> mutualValues = _transactionRegistryByTransactionHash[transactionsShortBlock.SyncBlockHeight].Where(kvp => transactionsShortBlock.TransactionHeaderHashes.Values.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return mutualValues.Count;
            }
        }

        public void ClearByConfirmed(RegistryShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                if (_transactionRegistryByTransactionHash.ContainsKey(transactionsShortBlock.SyncBlockHeight))
                {
                    Dictionary<IKey, RegistryRegisterBlock> mutualValues = _transactionRegistryByTransactionHash[transactionsShortBlock.SyncBlockHeight].Where(kvp => transactionsShortBlock.TransactionHeaderHashes.Values.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    foreach (IKey key in mutualValues.Keys)
                    {
                        _transactionRegistryByTransactionHash[transactionsShortBlock.SyncBlockHeight].Remove(key);
                        _transactionRegisterBlocksOrdered[transactionsShortBlock.SyncBlockHeight].Remove(_transactionOrderByTransactionHash[transactionsShortBlock.SyncBlockHeight][key]);
                        _transactionOrderByTransactionHash[transactionsShortBlock.SyncBlockHeight].Remove(key);
                    }
                }
            }
        }

        //TODO: need to understand whether it is needed to pass height of Sync Block or automatically take latest one?
        public SortedList<ushort, RegistryRegisterBlock> DequeueBulk(int maxCount)
        {
            SortedList<ushort, RegistryRegisterBlock> items = new SortedList<ushort, RegistryRegisterBlock>();
            lock(_sync)
            {
                ulong syncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;

                ushort order = 0;

                foreach (int orderKey in _transactionRegisterBlocksOrdered[syncBlockHeight].Keys)
                {
                    RegistryRegisterBlock transactionRegisterBlock = _transactionRegisterBlocksOrdered[syncBlockHeight][orderKey];

                    items.Add(order++, transactionRegisterBlock);

                    if (order == ushort.MaxValue)
                    {
                        break;
                    }
                }
            }

            return items;
        }
    }
}
