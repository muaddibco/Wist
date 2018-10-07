using System;
using System.Collections.Generic;
using System.Linq;
using Wist.BlockLattice.DataModel;
using Z.EntityFramework.Plus;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.BlockLattice.SQLite.DataAccess
{
    public partial class DataAccessService
    {

        #region Transactional Chain

        public TransactionalIdentity GetTransactionalIdentity(IKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if(accountIdentity == null)
            {
                return null;
            }

            return GetTransactionalIdentity(accountIdentity);
        }

        public TransactionalIdentity GetTransactionalIdentity(AccountIdentity accountIdentity)
        {
            if (accountIdentity == null)
            {
                throw new ArgumentNullException(nameof(accountIdentity));
            }

            lock (_sync)
            {
                return _dataContext.TransactionalIdentities.FirstOrDefault(g => g.Identity == accountIdentity);
            }
        }

        public bool IsTransactionalIdentityExist(IKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if(!_keyIdentityMap.ContainsKey(key))
            {
                return false;
            }

            return _dataContext.TransactionalIdentities.Any(g => g.Identity == _keyIdentityMap[key]);
        }

        public TransactionalIdentity AddTransactionalIdentity(AccountIdentity accountIdentity)
        {
            lock(_sync)
            {
                TransactionalIdentity transactionalIdentity = new TransactionalIdentity
                {
                    Identity = accountIdentity
                };

                _dataContext.TransactionalIdentities.Add(transactionalIdentity);

                return transactionalIdentity;
            }
        }

        public void AddTransactionalBlock(IKey key, ushort blockType, ulong blockHeight, byte[] blockContent)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (blockContent == null)
            {
                throw new ArgumentNullException(nameof(blockContent));
            }

            TransactionalIdentity transactionalIdentity = GetTransactionalIdentity(key);

            if (transactionalIdentity == null)
            {
                AccountIdentity accountIdentity = GetOrAddIdentity(key);
                transactionalIdentity = AddTransactionalIdentity(accountIdentity);
            }

            TransactionalBlock transactionalBlock = new TransactionalBlock()
            {
                Identity = transactionalIdentity,
                BlockContent = blockContent,
                BlockHeight = blockHeight,
                BlockType = blockType
            };

            lock (_sync)
            {
                _dataContext.TransactionalBlocks.Add(transactionalBlock);
            }
        }

        public TransactionalBlock GetLastTransactionalBlock(TransactionalIdentity transactionalIdentity)
        {
            if (transactionalIdentity == null)
            {
                throw new ArgumentNullException(nameof(transactionalIdentity));
            }

            lock (_sync)
            {
                return _dataContext.TransactionalBlocks.Where(b => b.Identity == transactionalIdentity).OrderByDescending(b => b.BlockHeight).FirstOrDefault();
            }
        }

        public TransactionalBlock GetLastTransactionalBlock(AccountIdentity accountIdentity)
        {
            TransactionalBlock transactionalBlock = null;

            if (accountIdentity == null)
            {
                throw new ArgumentNullException(nameof(accountIdentity));
            }

            TransactionalIdentity transactionalIdentity = GetTransactionalIdentity(accountIdentity);

            if (transactionalIdentity != null)
            {
                transactionalBlock = GetLastTransactionalBlock(transactionalIdentity);
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

            TransactionalIdentity transactionalIdentity = GetTransactionalIdentity(key);

            if (transactionalIdentity != null)
            {
                transactionalBlock = GetLastTransactionalBlock(transactionalIdentity);
            }


            return transactionalBlock;
        }

        public IEnumerable<TransactionalIdentity> GetAllTransctionalIdentities()
        {
            return _dataContext.TransactionalIdentities;
        }

        #endregion Transactional Chain
    }
}
