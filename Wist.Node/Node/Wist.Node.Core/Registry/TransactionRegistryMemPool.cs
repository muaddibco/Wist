using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Timers;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
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
        private readonly Dictionary<ulong, Dictionary<IKey, Dictionary<ulong, List<IKey>>>> _transactionHeadersPerAccountPerHeight;
        private readonly Dictionary<ulong, SortedDictionary<int, RegistryRegisterBlock>> _transactionRegisterBlocksOrdered;
        private readonly Dictionary<ulong, Dictionary<IKey, int>> _transactionOrderByTransactionHash;
        private readonly Dictionary<ulong, Dictionary<IKey, Tuple<IKey, ulong>>> _transactionSenderAndHeightByTransactionHash;
        private readonly Dictionary<ulong, int> _transactionsIndicies;

        // Key of this dictionary is hash of concatenation of Public Key of sender and Height of transaction

        private readonly Dictionary<ulong, Dictionary<ulong, HashSet<RegistryShortBlock>>> _transactionsShortBlocks;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ICryptoService _cryptoService;
        private readonly ITransactionsRegistryHelper _transactionsRegistryHelper;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private readonly ISynchronizationContext _synchronizationContext;
        private int _oldValue;
        private readonly object _sync = new object();

        public TransactionRegistryMemPool(ILoggerService loggerService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ICryptoService cryptoService, IStatesRepository statesRepository, ITransactionsRegistryHelper transactionsRegistryHelper)
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
            _transactionsIndicies = new Dictionary<ulong, int>();
            _transactionRegisterBlocksOrdered = new Dictionary<ulong, SortedDictionary<int, RegistryRegisterBlock>>();
            _transactionHeadersPerAccountPerHeight = new Dictionary<ulong, Dictionary<IKey, Dictionary<ulong, List<IKey>>>>();
            _transactionsShortBlocks = new Dictionary<ulong, Dictionary<ulong, HashSet<RegistryShortBlock>>>();
            _transactionOrderByTransactionHash = new Dictionary<ulong, Dictionary<IKey, int>>();
            _transactionSenderAndHeightByTransactionHash = new Dictionary<ulong, Dictionary<IKey, Tuple<IKey, ulong>>>();
            _cryptoService = cryptoService;
            _transactionsRegistryHelper = transactionsRegistryHelper;
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
        }
        
        public bool EnqueueTransactionRegisterBlock(RegistryRegisterBlock transactionRegisterBlock)
        {
            lock(_sync)
            {
                if(!_transactionsIndicies.ContainsKey(transactionRegisterBlock.SyncBlockHeight))
                {
                    _transactionsIndicies.Add(transactionRegisterBlock.SyncBlockHeight, 0);
                    _transactionRegisterBlocksOrdered.Add(transactionRegisterBlock.SyncBlockHeight, new SortedDictionary<int, RegistryRegisterBlock>());
                    _transactionHeadersPerAccountPerHeight.Add(transactionRegisterBlock.SyncBlockHeight, new Dictionary<IKey, Dictionary<ulong, List<IKey>>>());
                    _transactionOrderByTransactionHash.Add(transactionRegisterBlock.SyncBlockHeight, new Dictionary<IKey, int>());
                    _transactionSenderAndHeightByTransactionHash.Add(transactionRegisterBlock.SyncBlockHeight, new Dictionary<IKey, Tuple<IKey, ulong>>());
                }

                IKey transactionTwiceHashedKey = _transactionsRegistryHelper.GetTransactionRegistryTwiceHashedKey(transactionRegisterBlock);

                if (!_transactionOrderByTransactionHash[transactionRegisterBlock.SyncBlockHeight].ContainsKey(transactionTwiceHashedKey))
                {
                    _transactionRegisterBlocksOrdered[transactionRegisterBlock.SyncBlockHeight].Add(_transactionsIndicies[transactionRegisterBlock.SyncBlockHeight], transactionRegisterBlock);
                    _transactionOrderByTransactionHash[transactionRegisterBlock.SyncBlockHeight].Add(transactionTwiceHashedKey, _transactionsIndicies[transactionRegisterBlock.SyncBlockHeight]);

                    if(!_transactionHeadersPerAccountPerHeight[transactionRegisterBlock.SyncBlockHeight].ContainsKey(transactionRegisterBlock.Signer))
                    {
                        _transactionHeadersPerAccountPerHeight[transactionRegisterBlock.SyncBlockHeight].Add(transactionRegisterBlock.Signer, new Dictionary<ulong, List<IKey>>());
                    }

                    if(!_transactionHeadersPerAccountPerHeight[transactionRegisterBlock.SyncBlockHeight][transactionRegisterBlock.Signer].ContainsKey(transactionRegisterBlock.BlockHeight))
                    {
                        _transactionHeadersPerAccountPerHeight[transactionRegisterBlock.SyncBlockHeight][transactionRegisterBlock.Signer].Add(transactionRegisterBlock.BlockHeight, new List<IKey>());
                    }

                    _transactionHeadersPerAccountPerHeight[transactionRegisterBlock.SyncBlockHeight][transactionRegisterBlock.Signer][transactionRegisterBlock.BlockHeight].Add(transactionTwiceHashedKey);
                    _transactionSenderAndHeightByTransactionHash[transactionRegisterBlock.SyncBlockHeight].Add(transactionTwiceHashedKey, new Tuple<IKey, ulong>(transactionRegisterBlock.Signer, transactionRegisterBlock.BlockHeight));

                    _transactionsIndicies[transactionRegisterBlock.SyncBlockHeight]++;
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

        public byte[] GetConfidenceMask(RegistryShortBlock transactionsShortBlock, out byte[] bitMask)
        {
            lock(_sync)
            {
                bool[] bools = transactionsShortBlock.TransactionHeaderHashes.Select(kvp => _transactionOrderByTransactionHash[transactionsShortBlock.SyncBlockHeight].ContainsKey(kvp.Value)).ToArray();
                BitArray bitArray = bools.ToBitArray();

                bitMask = new byte[bitArray.Length / 8 + ((bitArray.Length % 8 > 0) ? 1 : 0)];

                bitArray.CopyTo(bitMask, 0);

                BigInteger bigIntegerSum = new BigInteger();
                int i = 0;
                foreach (var key in transactionsShortBlock.TransactionHeaderHashes.Keys)
                {
                    if(bools[i++])
                    {
                        int transactionHeaderOrder = _transactionOrderByTransactionHash[transactionsShortBlock.SyncBlockHeight][transactionsShortBlock.TransactionHeaderHashes[key]];
                        RegistryRegisterBlock registryRegisterBlock = _transactionRegisterBlocksOrdered[transactionsShortBlock.SyncBlockHeight][transactionHeaderOrder];
                        IKey registryRegisterBlockKey = _transactionsRegistryHelper.GetTransactionRegistryHashKey(registryRegisterBlock);
                        BigInteger bigInteger = new BigInteger(registryRegisterBlockKey.Value.ToArray());
                        bigIntegerSum += bigInteger;
                    }
                }

                //Dictionary<IKey, RegistryRegisterBlock> mutualValues = _transactionRegistryByTransactionHash[transactionsShortBlock.SyncBlockHeight].Where(kvp => transactionsShortBlock.TransactionHeaderHashes.Values.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                return bigIntegerSum.ToByteArray().Take(16).ToArray();
            }
        }

        public void ClearByConfirmed(RegistryShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                if (_transactionOrderByTransactionHash.ContainsKey(transactionsShortBlock.SyncBlockHeight))
                {
                    IEnumerable<IKey> mutualTransactionKeys = _transactionOrderByTransactionHash[transactionsShortBlock.SyncBlockHeight].Keys.Where(k => transactionsShortBlock.TransactionHeaderHashes.Values.Contains(k));

                    foreach (IKey key in mutualTransactionKeys)
                    {

                        _transactionHeadersPerAccountPerHeight[transactionsShortBlock.SyncBlockHeight][_transactionSenderAndHeightByTransactionHash[transactionsShortBlock.SyncBlockHeight][key].Item1].Remove(_transactionSenderAndHeightByTransactionHash[transactionsShortBlock.SyncBlockHeight][key].Item2);
                        if(_transactionHeadersPerAccountPerHeight[transactionsShortBlock.SyncBlockHeight][_transactionSenderAndHeightByTransactionHash[transactionsShortBlock.SyncBlockHeight][key].Item1].Count == 0)
                        {
                            _transactionHeadersPerAccountPerHeight[transactionsShortBlock.SyncBlockHeight].Remove(_transactionSenderAndHeightByTransactionHash[transactionsShortBlock.SyncBlockHeight][key].Item1);
                        }

                        _transactionSenderAndHeightByTransactionHash[transactionsShortBlock.SyncBlockHeight].Remove(key);
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

                if (_transactionRegisterBlocksOrdered.ContainsKey(syncBlockHeight))
                {
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
            }

            return items;
        }
    }
}
