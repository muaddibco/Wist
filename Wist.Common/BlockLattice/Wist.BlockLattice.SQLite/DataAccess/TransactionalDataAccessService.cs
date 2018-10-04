using System;
using System.Collections.Generic;
using System.Linq;
using Wist.BlockLattice.DataModel;
using Wist.Core.ExtensionMethods;
using Z.EntityFramework.Plus;
using Wist.BlockLattice.Core.Exceptions;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.BlockLattice.SQLite.DataAccess
{
    public partial class DataAccessService
    {

        #region Transactional Chain

        public TransactionalGenesis GetTransactionalGenesisBlock(IKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AccountIdentity accountIdentity = GetAccountIdentity(key);

            return GetTransactionalGenesisBlock(accountIdentity);
        }

        public TransactionalGenesis GetTransactionalGenesisBlock(AccountIdentity accountIdentity)
        {
            if (accountIdentity == null)
            {
                throw new ArgumentNullException(nameof(accountIdentity));
            }

            lock (_sync)
            {
                return _dataContext.TransactionalGenesises.FirstOrDefault(g => g.Identity == accountIdentity);
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

            lock (_sync)
            {
                TransactionalGenesis account = new TransactionalGenesis() { Identity = _keyIdentityMap[key], BlockContent = blockContent };
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

        public TransactionalBlock GetLastTransactionalBlock(TransactionalGenesis transactionalGenesis)
        {
            if (transactionalGenesis == null)
            {
                throw new ArgumentNullException(nameof(transactionalGenesis));
            }

            lock (_sync)
            {
                return _dataContext.TransactionalBlocks.Where(b => b.Genesis == transactionalGenesis).OrderByDescending(b => b.BlockOrder).FirstOrDefault();
            }
        }

        public void AddTransactionalBlock(IKey key, ushort blockType, byte[] blockContent)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (blockContent == null)
            {
                throw new ArgumentNullException(nameof(blockContent));
            }

            TransactionalGenesis transactionalGenesis = GetTransactionalGenesisBlock(key);

            if (transactionalGenesis == null)
            {
                throw new GenesisBlockNotFoundException(key.ToString());
            }

            TransactionalBlock transactionalBlockLast = GetLastTransactionalBlock(transactionalGenesis);
            byte[] contentHash = CryptoHelper.ComputeHash(blockContent);

            TransactionalBlock transactionalBlock = new TransactionalBlock()
            {
                Genesis = transactionalGenesis,
                BlockContent = blockContent,
                BlockOrder = transactionalBlockLast.BlockOrder + 1,
                BlockType = blockType
            };

            lock (_sync)
            {
                _dataContext.TransactionalBlocks.Add(transactionalBlock);
            }
        }

        public TransactionalBlock GetLastTransactionalBlock(AccountIdentity accountIdentity)
        {
            TransactionalBlock transactionalBlock = null;

            if (accountIdentity == null)
            {
                throw new ArgumentNullException(nameof(accountIdentity));
            }

            TransactionalGenesis transactionalGenesis = GetTransactionalGenesisBlock(accountIdentity);

            if (transactionalGenesis != null)
            {
                transactionalBlock = GetLastTransactionalBlock(transactionalGenesis);
            }

            return transactionalBlock;
        }

        public TransactionalBlock GetLastTransactionalBlock(IKey key)
        {
            TransactionalBlock transactionalBlock = null;

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            TransactionalGenesis transactionalGenesis = GetTransactionalGenesisBlock(key);

            if (transactionalGenesis != null)
            {
                transactionalBlock = GetLastTransactionalBlock(transactionalGenesis);
            }


            return transactionalBlock;
        }

        public IEnumerable<TransactionalGenesis> GetAllGenesisBlocks()
        {
            return _dataContext.TransactionalGenesises;
        }

        #endregion Transactional Chain
    }
}
