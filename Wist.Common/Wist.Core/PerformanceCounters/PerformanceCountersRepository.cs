using System;
using System.Collections.Generic;
using System.Linq;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.PerformanceCounters
{

    [RegisterDefaultImplementation(typeof(IPerformanceCountersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class PerformanceCountersRepository : IPerformanceCountersRepository
    {
        private readonly Dictionary<string, IPerformanceCountersCategoryBase> _performanceCountersCategoryBases;

        public PerformanceCountersRepository(IPerformanceCountersCategoryBase[] performanceCountersCategoryBases)
        {
            _performanceCountersCategoryBases = performanceCountersCategoryBases.ToDictionary(c => c.Name, c => c);
        }

        public T GetInstance<T>() where T : class, IPerformanceCountersCategoryBase
        {
            if (!(_performanceCountersCategoryBases.Values.FirstOrDefault(p => p.GetType() == typeof(T)) is T service))
            {
                throw new PerformanceCountersServiceNotSupportedException(typeof(T).FullName);
            }

            return service;
        }

        public IPerformanceCountersCategoryBase GetInstance(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_performanceCountersCategoryBases.ContainsKey(key))
            {
                throw new PerformanceCountersServiceNotSupportedException(key);
            }

            return _performanceCountersCategoryBases[key];
        }
    }
}
