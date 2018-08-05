using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IMemPoolsRepository : IRepository<IMemPool, Type>
    {
        IMemPool<T> GetInstance<T>();
    }
}
