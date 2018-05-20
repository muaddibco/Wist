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

namespace Wist.BlockLattice.SQLite
{
    [InitializationMandatory]
    [AutoLog]
    public class LatticeDataService : ISupportInitialization
    {
        private static readonly object _sync = new object();
        private static LatticeDataService _instance = null;

        private readonly DataContext _dataContext;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConfigurationService _configurationService;
        private bool _isSaving;

        private LatticeDataService(IConfigurationService configurationService)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _configurationService = configurationService;
            _dataContext = new DataContext((SQLiteConfiguration)_configurationService["sqlite"]);
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
                            _instance = ServiceLocator.Current.GetInstance<LatticeDataService>();
                            _instance.Initialize();
                        }
                    }
                }

                return _instance;
            }
        }

        public bool IsInitialized { get; private set; }

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

        public bool IsGenesisBlockExists(byte[] originalHash)
        {
            if (originalHash == null)
            {
                throw new ArgumentNullException(nameof(originalHash));
            }

            string originalHashValue = originalHash.ToHexString();

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

        public IAsyncEnumerable<TransactionalGenesis> GetAllGenesisBlocks()
        {
            return _dataContext.TransactionalGenesises.ToAsyncEnumerable();
        }

        #endregion Transactional Chain

        #region Common section
        public void Initialize()
        {
            if (IsInitialized)
                return;

            PeriodicTaskFactory.Start(() => 
            {
                if (_isSaving)
                    return;

                _isSaving = true;

                try
                {
                    if (_dataContext.ChangeTracker.HasChanges())
                        _dataContext.SaveChanges();
                }
                finally
                {
                    _isSaving = false;
                }
            }, 500, cancelToken: _cancellationTokenSource.Token);

            IsInitialized = true;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
        #endregion Common section
    }
}
