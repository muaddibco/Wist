using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry.SourceKeys
{
    public class AccountSourceKey : ITransactionSourceKey<AccountSourceKey>
    {
        public AccountSourceKey(SyncedBlockBase blockBase)
        {
            Signer = blockBase.Signer;
            Height = blockBase.BlockHeight;
        }


        public IKey Signer { get; private set; }
        public ulong Height { get; private set; }

        public bool Initialize(BlockBase blockBase)
        {
            if(blockBase is RegistryBlockBase registryBlockBase)
            {
                Signer = registryBlockBase.Signer;
                Height = registryBlockBase.BlockHeight;    
                return true;
            }

            return false;
        }

        public bool Equals(AccountSourceKey other)
        {
            return Signer.Equals(other.Signer) && Height == other.Height;
        }

        public bool Equals(AccountSourceKey x, AccountSourceKey y)
        {
            return x.Signer.Equals(y.Signer) && x.Height == y.Height;
        }

        public int GetHashCode(AccountSourceKey obj)
        {
            return obj.Signer.GetHashCode() ^ obj.Height.GetHashCode();
        }
    }
}
