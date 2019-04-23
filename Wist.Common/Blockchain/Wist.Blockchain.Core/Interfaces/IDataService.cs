using System.Collections.Generic;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Interfaces
{
    public interface IDataService<T, TKey> where T : Entity where TKey : IDataKey
    {
        void Initialize();

        void Add(T item);

        T Get(TKey key);

        void Update(TKey key, T item);

        IEnumerable<T> GetAll();

        IEnumerable<T> GetAll(TKey key);
    }
}
