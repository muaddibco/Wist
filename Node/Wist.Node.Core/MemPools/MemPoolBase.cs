using System;
using System.Collections.Generic;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.MemPools
{
    public abstract class MemPoolBase<T> : IMemPool<T>
    {
        public Type ElementType { get; } = typeof(T);

        public abstract bool Enqueue(T item);
        public abstract IEnumerable<T> DequeueBulk(int maxCount);
        public abstract void RemoveAll(IEnumerable<T> items);
    }
}
