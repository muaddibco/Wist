using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface IMemPool
    {
        Type ElementType { get; }
    }

    public interface IMemPool<T> : IMemPool
    {
        bool AddIfNotExist(T item);
    }
}
