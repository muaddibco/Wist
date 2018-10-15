using System;
using System.Collections.Generic;

namespace Wist.BlockLattice.Core.DataModel.Registry.SourceKeys
{
    public interface ITransactionSourceKey<T> : ITransactionSourceKey
    {
    }

    public interface ITransactionSourceKey : IEquatable<ITransactionSourceKey>, IEqualityComparer<ITransactionSourceKey>
    {

    }
}
