using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class TransactionsShortBlock : RegistryBlockBase, IEqualityComparer<TransactionsShortBlock>
    {
        public override ushort BlockType => BlockTypes.Registry_TransactionShortBlock;

        public override ushort Version => 1;

        public byte Round { get; set; }

        public SortedList<int, IKey> TransactionHeaderHashes { get; set; }

        public bool Equals(TransactionsShortBlock x, TransactionsShortBlock y)
        {
            if(x != null && y != null)
            {
                return x.SyncBlockHeight == y.SyncBlockHeight && x.Round == y.Round && x.Key.Equals(y.Key);
            }

            return x == null && y == null;
        }

        public int GetHashCode(TransactionsShortBlock obj)
        {
            int hash = obj.SyncBlockHeight.GetHashCode() ^ obj.Round.GetHashCode() ^ obj.Key.GetHashCode();

            hash += hash << 13;
            hash ^= hash >> 7;
            hash += hash << 3;
            hash ^= hash >> 17;
            hash += hash << 5;

            return hash;
        }
    }
}
