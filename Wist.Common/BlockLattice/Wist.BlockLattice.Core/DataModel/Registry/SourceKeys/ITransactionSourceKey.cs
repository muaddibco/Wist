using System;
using System.Collections.Generic;

namespace Wist.BlockLattice.Core.DataModel.Registry.SourceKeys
{
    public interface ITransactionSourceKey<T> : ITransactionSourceKey, IEquatable<T>, IEqualityComparer<T>
    {
    }

    public interface ITransactionSourceKey
    {

    }
}
