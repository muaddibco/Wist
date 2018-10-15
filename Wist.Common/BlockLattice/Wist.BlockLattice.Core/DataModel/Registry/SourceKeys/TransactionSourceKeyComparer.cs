using System.Collections.Generic;

namespace Wist.BlockLattice.Core.DataModel.Registry.SourceKeys
{
    public class TransactionSourceKeyComparer : IComparer<ITransactionSourceKey>, IEqualityComparer<ITransactionSourceKey>
    {
        public int Compare(ITransactionSourceKey x, ITransactionSourceKey y)
        {
            if(x == null && y == null)
            {
                return 0;
            }

            if(x == null)
            {
                return -1;
            }

            if(y == null)
            {
                return 1;
            }

            return x.Equals(y) ? 0 : x.GetHashCode() > y.GetHashCode() ? 1 : -1;
        }

        public bool Equals(ITransactionSourceKey x, ITransactionSourceKey y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            return x?.Equals(y) ?? false;
        }

        public int GetHashCode(ITransactionSourceKey obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}
