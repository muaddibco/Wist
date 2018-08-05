using System;
using System.Collections.Generic;
using System.Linq;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Exceptions;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.MemPools
{
    [RegisterDefaultImplementation(typeof(IMemPoolsRepository), Lifetime = LifetimeManagement.Singleton)]
    public class MemPoolsRepository : IMemPoolsRepository
    {
        private readonly Dictionary<Type, IMemPool> _memPools;

        public MemPoolsRepository(IMemPool[] memPools)
        {
            _memPools = memPools.ToDictionary(m => m.ElementType, m => m);
        }

        public IMemPool<T> GetInstance<T>()
        {
            return GetInstance(typeof(T)) as IMemPool<T>;
        }

        public IMemPool GetInstance(Type key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if(!_memPools.ContainsKey(key))
            {
                throw new MemPoolOfElementsNotSupportedException(key);
            }

            return _memPools[key];
        }
    }
}
