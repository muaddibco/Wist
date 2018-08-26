using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.ExtensionMethods;
using Z.EntityFramework.Plus;
using Wist.BlockLattice.Core.Exceptions;
using Wist.Core.Cryptography;
using Wist.Core.Configuration;
using CommonServiceLocator;
using Wist.BlockLattice.SQLite.Configuration;
using Wist.Core.Models;
using Wist.Core.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Wist.BlockLattice.SQLite
{
    [AutoLog]
    public class LatticeDataService
    {
        private static readonly object _sync = new object();
        
        private static LatticeDataService _instance = null;
        private Dictionary<IKey, AccountIdentity> _keyIdentityMap;
        private Dictionary<IKey, Node> _keyToNodeMap;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        private readonly DataContext _dataContext;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConfigurationService _configurationService;
        private bool _isSaving;

        private LatticeDataService(IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _configurationService = configurationService;
            _dataContext = new DataContext(_configurationService.Get<ISQLiteConfiguration>());
            _dataContext.ChangeTracker.StateChanged += (s, e) =>
            {
                AccountIdentity accountIdentity = e.Entry.Entity as AccountIdentity;
            };
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public static LatticeDataService Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(_sync)
                    {
                        if(_instance == null)
                        {
                            _instance = new LatticeDataService(ServiceLocator.Current.GetInstance<IConfigurationService>(), ServiceLocator.Current.GetInstance<IIdentityKeyProvidersRegistry>());
                            _instance.Initialize();
                        }
                    }
                }

                return _instance;
            }
        }

        public bool IsInitialized { get; private set; }

        #region Account Seeds

        internal bool AddSeed(IKey key, byte[] seed)
        {
            AccountIdentity identity = GetAccountIdentity(key);

            if (identity != null)
            {
                lock (_sync)
                {
                    AccountSeed accountSeed = _dataContext.AccountSeeds.FirstOrDefault(s => s.Identity == identity);
                    if(accountSeed == null)
                    {
                    accountSeed = new AccountSeed { Identity = identity, Seed = seed};
                        _dataContext.AccountSeeds.Add(accountSeed);
                    }

                    return true;
                }
            }

            return false;
        }

        #endregion Account Seeds

        #region Account Identities

        public void LoadAllIdentities()
        {
            lock (_sync)
            {
                _keyIdentityMap = _dataContext.AccountIdentities.ToDictionary(i => _identityKeyProvider.GetKey(i.PublicKey), i => i);
            }
        }

        public IEnumerable<IKey> GetAllAccountIdentities()
        {
            return _keyIdentityMap.Select(m => m.Key).ToList();
        }

        public AccountIdentity GetAccountIdentity(IKey key)
        {
            if (_keyIdentityMap.ContainsKey(key))
            {
                return _keyIdentityMap[key];
            }

            return null;
        }

        public bool AddIdentity(IKey key)
        {
            AccountIdentity identity = GetAccountIdentity(key);

            if(identity == null)
            {
                identity = new AccountIdentity { PublicKey = key.Value };

                lock (_sync)
                {
                    _dataContext.AccountIdentities.Add(identity);
                    _keyIdentityMap.Add(key, identity);
                }

                return true;
            }

            return false;
        }

        #endregion Account Identities

        #region Nodes

        public bool AddNode(IKey key, string ipAddressExpression = null)
        {
            IPAddress ipAddress;
            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out ipAddress))
            {
                return AddNode(key, ipAddress);
            }

            return false;
        }

        public bool AddNode(IKey key, IPAddress ipAddress)
        {
            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if(accountIdentity == null)
            {
                AddIdentity(key);

                accountIdentity = GetAccountIdentity(key);
            }

            if (accountIdentity != null)
            {
                lock (_sync)
                {
                    Node node = _dataContext.Nodes.FirstOrDefault(n => n.Identity == accountIdentity);

                    if (node == null)
                    {

                        node = new Node { Identity = accountIdentity, IPAddress = ipAddress.GetAddressBytes() };
                        _dataContext.Nodes.Add(node);
                        _keyToNodeMap.Add(key, node);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool UpdateNode(IKey key, string ipAddressExpression = null)
        {
            IPAddress ipAddress;

            if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out ipAddress))
            {
                return UpdateNode(key, ipAddress);
            }

            return false;
        }

        public bool UpdateNode(IKey key, IPAddress ipAddress)
        {
            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if (accountIdentity != null)
            {
                lock (_sync)
                {
                    Node node = _dataContext.Nodes.FirstOrDefault(n => n.Identity == accountIdentity);

                    if (node != null)
                    {
                        node.IPAddress = ipAddress.GetAddressBytes();
                        _dataContext.Update<Node>(node);
                        _keyToNodeMap[key].IPAddress = ipAddress.GetAddressBytes();
                        return true;
                    }
                }
            }

            return false;
        }

        public void LoadAllKnownNodeIPs()
        {
            lock (_sync)
            {
                _keyToNodeMap = _dataContext.Nodes.ToDictionary(i => _identityKeyProvider.GetKey(i.Identity.PublicKey), i => i);
            }
        }

        public IPAddress GetNodeIpAddress(IKey key)
        {
            if(_keyToNodeMap.ContainsKey(key))
            {
                return new IPAddress(_keyToNodeMap[key].IPAddress);
            }

            return IPAddress.None;
        }

        public IEnumerable<Node> GetAllNodes()
        {
            return _keyToNodeMap.Values;
        }

        public Node GetNode(IKey key)
        {
            if(_keyToNodeMap.ContainsKey(key))
            {
                return _keyToNodeMap[key];
            }

            return null;
        }
        #endregion Nodes

        #region Accounts Chain



        #endregion Accounts Chain

        #region Transactional Chain

        public TransactionalGenesisModification GetTransactionalGenesisModificationBlock(IKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AccountIdentity accountIdentity = GetAccountIdentity(key);

            return GetTransactionalGenesisModificationBlock(accountIdentity);
        }

        public TransactionalGenesisModification GetTransactionalGenesisModificationBlock(AccountIdentity accountIdentity)
        {
            if (accountIdentity == null)
            {
                throw new ArgumentNullException(nameof(accountIdentity));
            }

            lock (_sync)
            {
                return _dataContext.TransactionalGenesisModifications.FirstOrDefault(g => g.Identity == accountIdentity);
            }
        }

        public void CreateTransactionalGenesisBlock(IKey key, ushort version, byte[] blockContent)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (IsGenesisBlockExists(key))
            {
                throw new GenesisBlockAlreadyExistException(key.ToString());
            }

            AddIdentity(key);

            lock(_sync)
            {
                TransactionalGenesis account = new TransactionalGenesis() { Identity = _keyIdentityMap[key], Version = version, BlockContent = blockContent };
                _dataContext.TransactionalGenesises.AddAsync(account);
            }
        }

        public bool IsGenesisBlockExists(IKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _dataContext.TransactionalGenesises.Any(g => g.Identity == _keyIdentityMap[key]);
        }

        public TransactionalBlock GetLastTransactionalBlock(TransactionalGenesisModification transactionalGenesisModification)
        {
            if (transactionalGenesisModification == null)
            {
                throw new ArgumentNullException(nameof(transactionalGenesisModification));
            }

            lock (_sync)
            {
                return _dataContext.TransactionalBlocks.Where(b => b.Genesis == transactionalGenesisModification).OrderByDescending(b => b.BlockOrder).FirstOrDefault();
            }
        }

        public void AddTransactionalBlock(IKey key, ushort version, ushort blockType, byte[] blockContent)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (blockContent == null)
            {
                throw new ArgumentNullException(nameof(blockContent));
            }

            TransactionalGenesisModification transactionalGenesisModification = GetTransactionalGenesisModificationBlock(key);

            if(transactionalGenesisModification == null)
            {
                throw new GenesisBlockNotFoundException(key.Value.ToHexString());
            }

            TransactionalBlock transactionalBlockLast = GetLastTransactionalBlock(transactionalGenesisModification);
            byte[] contentHash = CryptoHelper.ComputeHash(blockContent);

            TransactionalBlock transactionalBlock = new TransactionalBlock()
            {
                Genesis = transactionalGenesisModification,
                BlockContent = blockContent,
                BlockOrder = transactionalBlockLast.BlockOrder + 1,
                BlockType = blockType
            };

            lock(_sync)
            {
                _dataContext.TransactionalBlocks.Add(transactionalBlock);
            }
        }

        public TransactionalGenesisModification GetLastTransactionalGenesisModification(IKey key)
        {
            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if(accountIdentity != null && _dataContext.TransactionalGenesisModifications.Any(g => g.Identity == accountIdentity))
            {
                lock(_sync)
                {
                    return _dataContext.TransactionalGenesisModifications.Where(g1 => g1.TransactionalGenesis == _dataContext.TransactionalGenesisModifications.FirstOrDefault(g => g.Identity == accountIdentity).TransactionalGenesis).OrderByDescending(g => g.BlockOrder).FirstOrDefault();
                }
            }

            return null;
        }

        public TransactionalGenesisModification GetLastTransactionalGenesisModification(TransactionalGenesis genesis)
        {
            lock(_sync)
            {
                return _dataContext.TransactionalGenesisModifications.Where(g => g.TransactionalGenesis == genesis).OrderByDescending(g => g.BlockOrder).FirstOrDefault();
            }
        }

        public TransactionalBlock GetLastTransactionalBlock(AccountIdentity accountIdentity, bool forSpecific = true)
        {
            TransactionalBlock transactionalBlock = null;

            if (accountIdentity == null)
            {
                throw new ArgumentNullException(nameof(accountIdentity));
            }

            if (forSpecific)
            {
                TransactionalGenesisModification transactionalGenesisModification = GetTransactionalGenesisModificationBlock(accountIdentity);

                if (transactionalGenesisModification != null)
                {
                    transactionalBlock = GetLastTransactionalBlock(transactionalGenesisModification);
                }
            }
            else
            {
                TransactionalGenesis transactionalGenesis = _dataContext.TransactionalGenesisModifications.FirstOrDefault(b => b.Identity == accountIdentity)?.TransactionalGenesis;

                if (transactionalGenesis == null)
                {
                    throw new GenesisBlockNotFoundException(accountIdentity.PublicKey.ToHexString());
                }

                TransactionalGenesisModification lastTransactionalGenesisModificationWithTransactions = _dataContext.TransactionalGenesisModifications.Where(b => b.TransactionalGenesis == transactionalGenesis && b.TransactionBlocks.Any()).OrderByDescending(b => b.BlockOrder).FirstOrDefault();

                if (lastTransactionalGenesisModificationWithTransactions != null)
                {
                    transactionalBlock = _dataContext.TransactionalBlocks.Where(b => b.Genesis == lastTransactionalGenesisModificationWithTransactions).OrderByDescending(b => b.BlockOrder).FirstOrDefault();
                }
            }

            return transactionalBlock;
        }

        public TransactionalBlock GetLastTransactionalBlock(IKey key, bool forSpecific = true)
        {
            TransactionalBlock transactionalBlock = null;

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (forSpecific)
            {
                TransactionalGenesisModification transactionalGenesisModification = GetTransactionalGenesisModificationBlock(key);

                if (transactionalGenesisModification != null)
                {
                    transactionalBlock = GetLastTransactionalBlock(transactionalGenesisModification);
                }
            }
            else
            {
                AccountIdentity accountIdentity = GetAccountIdentity(key);

                TransactionalGenesis transactionalGenesis = _dataContext.TransactionalGenesisModifications.FirstOrDefault(b => b.Identity == accountIdentity)?.TransactionalGenesis;

                if (transactionalGenesis == null)
                {
                    throw new GenesisBlockNotFoundException(key.Value.ToHexString());
                }

                TransactionalGenesisModification lastTransactionalGenesisModificationWithTransactions = _dataContext.TransactionalGenesisModifications.Where(b => b.TransactionalGenesis == transactionalGenesis && b.TransactionBlocks.Any()).OrderByDescending(b => b.BlockOrder).FirstOrDefault();

                if (lastTransactionalGenesisModificationWithTransactions != null)
                {
                    transactionalBlock = _dataContext.TransactionalBlocks.Where(b => b.Genesis == lastTransactionalGenesisModificationWithTransactions).OrderByDescending(b => b.BlockOrder).FirstOrDefault();
                }
            }

            return transactionalBlock;
        }

        public IEnumerable<TransactionalGenesis> GetAllGenesisBlocks()
        {
            return _dataContext.TransactionalGenesises;
        }

        #endregion Transactional Chain

        #region Synchronization

        public void AddSynchronizationBlock(ulong blockHeight, DateTime receiveTime, DateTime medianTime, byte[] content)
        {
            lock(_sync)
            {
                _dataContext.SynchronizationBlocks.Add(new SynchronizationBlock() { SynchronizationBlockId = blockHeight, ReceiveTime = receiveTime, MedianTime = medianTime, BlockContent = content });
            }
        }

        public SynchronizationBlock GetLastSynchronizationBlock()
        {
            lock(_sync)
            {
                return _dataContext.SynchronizationBlocks.OrderByDescending(b => b.SynchronizationBlockId).FirstOrDefault();
            }
        }

        #endregion Synchronization

        #region Common section
        public void Initialize()
        {
            if (IsInitialized)
                return;

            _dataContext.Database.EnsureCreated();
            _dataContext.EnsureConfigurationCompleted();

            PeriodicTaskFactory.Start(() => 
            {
                if (_isSaving)
                    return;

                lock (_sync)
                {
                    if (_isSaving)
                        return;

                    _isSaving = true;

                    try
                    {
                        if (_dataContext.ChangeTracker.HasChanges())
                        {
                            _dataContext.SaveChanges();
                        }
                    }
                    finally
                    {
                        _isSaving = false;
                    }
                }
            }, 1000, cancelToken: _cancellationTokenSource.Token);

            IsInitialized = true;
        }

        public void EnsureChangesSaved()
        {
            while (_dataContext.ChangeTracker.HasChanges()) Thread.Sleep(500);
        }

        internal void WipeAll()
        {
            lock (_sync)
            {
                _dataContext.Database.EnsureDeleted();
                _dataContext.Database.EnsureCreated();
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }

        #endregion Common section
    }
}
