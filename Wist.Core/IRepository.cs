using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core
{
    public interface IRepository<T, TKey>
    {
        T GetInstance(TKey key);
    }

    public interface IRepository<T, TKey1, TKey2>
    {
        T GetInstance(TKey1 key1, TKey2 key2);
    }
}
