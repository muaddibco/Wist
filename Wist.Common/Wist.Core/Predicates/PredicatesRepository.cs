using System;
using System.Collections.Generic;
using System.Linq;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;
using Wist.Core.Properties;

namespace Wist.Core.Predicates
{
    [RegisterDefaultImplementation(typeof(IPredicatesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class PredicatesRepository : IPredicatesRepository
    {
        private readonly Dictionary<string, IPredicate> _predicates;

        public PredicatesRepository(IPredicate[] predicates)
        {
            _predicates = predicates.ToDictionary(p => p.Name, p => p);
        }

        public IPredicate GetInstance(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException(Resources.ERR_PREDICATE_NAME_IS_MANDATORY, nameof(key));
            }

            if (!_predicates.ContainsKey(key))
            {
                throw new PredicateIsNotSupportedException(key);
            }

            return _predicates[key];
        }
    }
}
