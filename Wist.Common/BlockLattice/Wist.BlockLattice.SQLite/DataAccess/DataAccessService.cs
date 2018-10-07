using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Wist.BlockLattice.DataModel;
using Wist.Core;
using Wist.Core.Aspects;
using Wist.Core.Configuration;
using CommonServiceLocator;
using Wist.BlockLattice.SQLite.Configuration;
using Wist.Core.Identity;
using Wist.Core.Logging;
using System;

namespace Wist.BlockLattice.SQLite.DataAccess
{
    [AutoLog]
    public partial class DataAccessService
    {
        private static readonly object _sync = new object();
        
        private static DataAccessService _instance = null;
        private Dictionary<IKey, AccountIdentity> _keyIdentityMap;
        private Dictionary<IKey, NodeRecord> _keyToNodeMap;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        private readonly DataContext _dataContext;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _logger;
        private bool _isSaving;

        private DataAccessService(IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ILoggerService loggerService)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _configurationService = configurationService;
            _dataContext = new DataContext(_configurationService.Get<ISQLiteConfiguration>());
            _dataContext.ChangeTracker.StateChanged += (s, e) =>
            {
                AccountIdentity accountIdentity = e.Entry.Entity as AccountIdentity;
            };
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _logger = loggerService.GetLogger(nameof(DataAccessService));
        }

        public static DataAccessService Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(_sync)
                    {
                        if(_instance == null)
                        {
                            _instance = new DataAccessService(ServiceLocator.Current.GetInstance<IConfigurationService>(), ServiceLocator.Current.GetInstance<IIdentityKeyProvidersRegistry>(), ServiceLocator.Current.GetInstance<ILoggerService>());
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
                    if (accountSeed == null)
                    {
                        accountSeed = new AccountSeed { Identity = identity, Seed = seed };
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

        public AccountIdentity GetOrAddIdentity(IKey key)
        {
            AccountIdentity identity = GetAccountIdentity(key);

            if(identity == null)
            {
                identity = new AccountIdentity { PublicKey = key.Value.ToArray() };

                lock (_sync)
                {
                    _dataContext.AccountIdentities.Add(identity);
                    _keyIdentityMap.Add(key, identity);
                }
            }

            return identity;
        }

        #endregion Account Identities

        #region Accounts Chain



        #endregion Accounts Chain

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
                    catch(Exception ex)
                    {
                        _logger.Error("Failure during saving data to database", ex);
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
