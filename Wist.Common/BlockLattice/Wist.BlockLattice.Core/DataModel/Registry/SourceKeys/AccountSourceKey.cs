using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry.SourceKeys
{
    public class AccountSourceKey : ITransactionSourceKey<AccountSourceKey>
    {
        private readonly SyncedBlockBase _blockBase;

        public AccountSourceKey(SyncedBlockBase blockBase)
        {
            _blockBase = blockBase;
        }

        public IKey Signer => _blockBase.Signer;

        public ulong Height => _blockBase.BlockHeight;

        public bool Equals(ITransactionSourceKey x, ITransactionSourceKey y)
        {
            if(x == null && y == null)
            {
                return true;
            }

            if(x is AccountSourceKey accountSourceKey1 && y is AccountSourceKey accountSourceKey2)
            {
                return accountSourceKey1.Signer.Equals(accountSourceKey2.Signer) && accountSourceKey1.Height == accountSourceKey2.Height;

            }

            return false;
        }

        public bool Equals(ITransactionSourceKey other)
        {
            if(other is AccountSourceKey accountSourceKey)
            {
                return Signer.Equals(accountSourceKey.Signer) && Height == accountSourceKey.Height;
            }

            return false;
        }

        public int GetHashCode(ITransactionSourceKey obj)
        {
            if (obj is AccountSourceKey accountSourceKey)
            {
                return accountSourceKey.Signer.GetHashCode() ^ accountSourceKey.Height.GetHashCode();
            }

            return 0;
        }

        public override int GetHashCode()
        {
            return Signer.GetHashCode() ^ Height.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is AccountSourceKey accountSourceKey)
            {
                return Signer.Equals(accountSourceKey.Signer) && Height == accountSourceKey.Height;
            }

            return false;

        }
    }
}
