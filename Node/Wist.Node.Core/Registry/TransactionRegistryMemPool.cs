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
        private readonly Dictionary<ulong, SortedDictionary<int, TransactionRegisterBlock>> _transactionRegisterBlocksOrdered;
        private readonly Dictionary<ulong, int> _transactionsCounters;

        // Key of this dictionary is hash of concatenation of Public Key of sender and Height of transaction
        private readonly Dictionary<ulong, Dictionary<IKey, TransactionRegisterBlock>> _transactionRegistryByTransactionHash;
        private readonly Dictionary<ulong, Dictionary<IKey, int>> _transactionOrderByTransactionHash;

        private readonly Dictionary<ulong, Dictionary<byte, HashSet<TransactionsShortBlock>>> _transactionsShortBlocks;
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

            _transactionHashKey = identityKeyProvidersRegistry.GetInstance("TransactionRegistry");
            _logger = loggerService.GetLogger(nameof(TransactionRegistryMemPool));
            _transactionsCounters = new Dictionary<ulong, int>();
            _transactionRegisterBlocksOrdered = new Dictionary<ulong, SortedDictionary<int, TransactionRegisterBlock>>();
            _transactionRegistryByTransactionHash = new Dictionary<ulong, Dictionary<IKey, TransactionRegisterBlock>>();
            _transactionsShortBlocks = new Dictionary<ulong, Dictionary<byte, HashSet<TransactionsShortBlock>>>();
            _transactionOrderByTransactionHash = new Dictionary<ulong, Dictionary<IKey, int>>();
            _cryptoService = cryptoService;
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
        }
        
        public bool EnqueueTransactionRegisterBlock(TransactionRegisterBlock transactionRegisterBlock)
        {
            lock(_sync)
            {
                if(!_transactionRegistryByTransactionHash.ContainsKey(transactionRegisterBlock.SyncBlockHeight))
                {
                    _transactionsCounters.Add(transactionRegisterBlock.SyncBlockHeight, 0);
                    _transactionRegisterBlocksOrdered.Add(transactionRegisterBlock.SyncBlockHeight, new SortedDictionary<int, TransactionRegisterBlock>());
                    _transactionRegistryByTransactionHash.Add(transactionRegisterBlock.SyncBlockHeight, new Dictionary<IKey, TransactionRegisterBlock>());
                    _transactionOrderByTransactionHash.Add(transactionRegisterBlock.SyncBlockHeight, new Dictionary<IKey, int>());
                }

                IKey key = GetTransactionRegistryHashKey(transactionRegisterBlock);

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

        public int GetConfidenceRate(TransactionsShortBlock transactionsShortBlock)
        {
            lock(_sync)
            {
                Dictionary<IKey, TransactionRegisterBlock> mutualValues = _transactionRegistryByTransactionHash[transactionsShortBlock.SyncBlockHeight].Where(kvp => transactionsShortBlock.TransactionHeaderHashes.Values.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return mutualValues.Count;
            }
        }

        public void ClearByConfirmed(TransactionsShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                if (_transactionRegistryByTransactionHash.ContainsKey(transactionsShortBlock.SyncBlockHeight))
                {
                    Dictionary<IKey, TransactionRegisterBlock> mutualValues = _transactionRegistryByTransactionHash[transactionsShortBlock.SyncBlockHeight].Where(kvp => transactionsShortBlock.TransactionHeaderHashes.Values.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

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
        public SortedList<ushort, TransactionRegisterBlock> DequeueBulk(int maxCount)
        {
            SortedList<ushort, TransactionRegisterBlock> items = new SortedList<ushort, TransactionRegisterBlock>();
            lock(_sync)
            {
                ulong syncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;

                ushort order = 0;

                foreach (int orderKey in _transactionRegisterBlocksOrdered[syncBlockHeight].Keys)
                {
                    TransactionRegisterBlock transactionRegisterBlock = _transactionRegisterBlocksOrdered[syncBlockHeight][orderKey];

                    items.Add(order++, transactionRegisterBlock);

                    if (order == ushort.MaxValue)
                    {
                        break;
                    }
                }
            }

            return items;
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
