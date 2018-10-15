using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Registry.SourceKeys;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public interface ITransactionRegistryBlock<T>
    {
        ITransactionSourceKey<T> TransactionSourceKey { get; }
    }
}
