using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.DataModel.Registry.SourceKeys
{
    public interface ITransactionSourceKey<T> : IEquatable<T>, IEqualityComparer<T>
    {
    }
}
