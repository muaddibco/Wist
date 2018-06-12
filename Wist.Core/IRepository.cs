using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core
{
    public interface IRepository<T, TKey>
    {
        T GetInstance(TKey key);
    }

    public interface IBulkRepository<T>
    {
        void RegisterInstance(T obj);

        IEnumerable<T> GetBulkInstances();
    }
}
