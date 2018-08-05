using System;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.MemPools
{
    public abstract class MemPoolBase<T> : IMemPool<T>
    {
        public Type ElementType { get; } = typeof(T);

        public abstract bool AddIfNotExist(T item);
    }
}
