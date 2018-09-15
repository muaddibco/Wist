using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class RegistryShortBlock : RegistryBlockBase, IEqualityComparer<RegistryShortBlock>
    {
        public override ushort BlockType => BlockTypes.Registry_ShortBlock;

        public override ushort Version => 1;

        public SortedList<ushort, IKey> TransactionHeaderHashes { get; set; }

        public bool Equals(RegistryShortBlock x, RegistryShortBlock y)
        {
            if(x != null && y != null)
            {
                return x.SyncBlockHeight == y.SyncBlockHeight && x.BlockHeight == y.BlockHeight && x.Signer.Equals(y.Signer);
            }

            return x == null && y == null;
        }

        public int GetHashCode(RegistryShortBlock obj)
        {
            int hash = obj.SyncBlockHeight.GetHashCode() ^ obj.BlockHeight.GetHashCode() ^ obj.Signer.GetHashCode();

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;

            return hash;
        }
    }
}
