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
        private readonly IIdentityKeyProvider _identityKeyProvider;

        private readonly DataContext _dataContext;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConfigurationService _configurationService;
        private bool _isSaving;

        private LatticeDataService(IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _configurationService = configurationService;
            _dataContext = new DataContext(_configurationService.Get<SQLiteConfiguration>());
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
            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if(accountIdentity != null)
            {
                lock(_sync)
                {
                    Node node = _dataContext.Nodes.FirstOrDefault(n => n.Identity == accountIdentity);

                    if(node == null)
                    {
                        IPAddress ipAddress;

                        if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out ipAddress))
                        {
                            node = new Node { Identity = accountIdentity, IPAddress = ipAddress.GetAddressBytes() };
                            _dataContext.Nodes.Add(node);
                        }
                    }
                }
            }

            return false;
        }

        public bool UpdateNode(IKey key, string ipAddressExpression = null)
        {
            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if (accountIdentity != null)
            {
                lock (_sync)
                {
                    Node node = _dataContext.Nodes.FirstOrDefault(n => n.Identity == accountIdentity);

                    if (node != null)
                    {
                        IPAddress ipAddress;

                        if (IPAddress.TryParse(ipAddressExpression ?? "127.0.0.1", out ipAddress))
                        {
                            node.IPAddress = ipAddress.GetAddressBytes();
                            _dataContext.Update<Node>(node);
                        }
                    }
                }
            }

            return false;
        }

        #endregion Nodes

        #region Accounts Chain



        #endregion Accounts Chain

        #region Transactional Chain

        public TransactionalGenesis GetTransactionalGenesisBlock(string originalHash)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            return _dataContext.TransactionalGenesises.FirstOrDefault(g => g.OriginalHash.Equals(originalHash));
        }

        public TransactionalGenesis GetTransactionalGenesisBlock(byte[] originalHash)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            string originalHashValue = originalHash.ToHexString();

            return _dataContext.TransactionalGenesises.FirstOrDefault(g => g.OriginalHash.Equals(originalHashValue));
        }

        public void CreateTransactionalGenesisBlock(byte[] originalHash)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            if (originalHash.Length != 64)
            {
                throw new ArgumentException("Hash value must be 64 bytes length");
            }

            string originalHashValue = originalHash.ToHexString();
            if (IsGenesisBlockExists(originalHashValue))
            {
                throw new GenesisBlockAlreadyExistException(originalHashValue);
            }

            TransactionalGenesis account = new TransactionalGenesis() { OriginalHash = originalHashValue };
            
            _dataContext.TransactionalGenesises.AddAsync(account);
        }

        public bool IsGenesisBlockExists(IKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            string originalHashValue = key.Value.ToHexString();

            return _dataContext.TransactionalGenesises.Any(g => g.OriginalHash.Equals(originalHashValue));
        }

        public bool IsGenesisBlockExists(string originalHash)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            return _dataContext.TransactionalGenesises.Any(g => g.OriginalHash.Equals(originalHash));
        }

        public TransactionalBlock GetLastTransactionalBlock(byte[] originalHash)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            string hashValue = originalHash.ToHexString();

            return _dataContext.TransactionalBlocks.Where(b => b.TransactionalGenesis.OriginalHash == hashValue).OrderByDescending(b => b.BlockOrder).FirstOrDefault();
        }

        public TransactionalBlock GetLastTransactionalBlock(string originalHash)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            //TODO: need to examine performance
            if(!IsGenesisBlockExists(originalHash))
            {
                throw new GenesisBlockNotFoundException(originalHash);
            }

            return _dataContext.TransactionalBlocks.Where(b => b.TransactionalGenesis.OriginalHash == originalHash).OrderByDescending(b => b.BlockOrder).FirstOrDefault();
        }

        public void AddTransactionalBlock(byte[] originalHash, byte[] nbackHash, ushort blockType, byte[] blockContent)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            if (nbackHash == null)
            {
                throw new ArgumentNullException(nameof(nbackHash));
            }

            if (blockContent == null)
            {
                throw new ArgumentNullException(nameof(blockContent));
            }

            string originalHashValue = originalHash.ToHexString();

            TransactionalGenesis transactionalGenesisBlock = GetTransactionalGenesisBlock(originalHashValue);

            if(transactionalGenesisBlock == null)
            {
                throw new GenesisBlockNotFoundException(originalHashValue);
            }

            TransactionalBlock transactionalBlockLast = GetLastTransactionalBlock(originalHashValue);
            byte[] contentHash = CryptoHelper.ComputeHash(blockContent);

            TransactionalBlock transactionalBlock = new TransactionalBlock()
            {
                TransactionalGenesis = transactionalGenesisBlock,
                NBackHash = nbackHash.ToHexString(),
                BlockContent = blockContent,
                BlockOrder = transactionalBlockLast.BlockOrder + 1,
                BlockType = blockType
            };
        }

        /// <summary>
        /// Returns last block of certain type of chain identified by hashValue
        /// </summary>
        /// <param name="originalHash">HASH value identifying chain</param>
        /// <param name="blockType"></param>
        /// <returns></returns>
        public TransactionalBlock GetLastBlockModification(byte[] originalHash, ushort blockType)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            string originalHashValue = originalHash.ToHexString();

            TransactionalGenesis transactionalGenesisBlock = GetTransactionalGenesisBlock(originalHashValue);

            if (transactionalGenesisBlock == null)
            {
                throw new GenesisBlockNotFoundException(originalHashValue);
            }

            TransactionalBlock transactionalBlock = _dataContext.TransactionalBlocks.Where(b => b.TransactionalGenesis.OriginalHash == originalHashValue && b.BlockType == blockType).OrderBy(b => b.BlockOrder).LastOrDefault();

            return transactionalBlock;
        }

        /// <summary>
        /// Returns last block of certain type of chain identified by hashValue
        /// </summary>
        /// <param name="originalHash">HASH value identifying chain</param>
        /// <param name="blockType"></param>
        /// <returns></returns>
        public TransactionalBlock GetLastBlockModification(string originalHash, ushort blockType)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            TransactionalGenesis transactionalGenesisBlock = GetTransactionalGenesisBlock(originalHash);

            if (transactionalGenesisBlock == null)
            {
                throw new GenesisBlockNotFoundException(originalHash);
            }

            TransactionalBlock transactionalBlock = _dataContext.TransactionalBlocks.Where(b => b.TransactionalGenesis.OriginalHash == originalHash && b.BlockType == blockType).OrderBy(b => b.BlockOrder).LastOrDefault();

            return transactionalBlock;
        }

        public IEnumerable<TransactionalGenesis> GetAllGenesisBlocks()
        {
            return _dataContext.TransactionalGenesises;
        }

        #endregion Transactional Chain

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
