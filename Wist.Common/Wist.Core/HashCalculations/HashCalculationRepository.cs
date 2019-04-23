using Unity;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.HashCalculations
{
    [RegisterDefaultImplementation(typeof(IHashCalculationsRepository), Lifetime = LifetimeManagement.Singleton)]
    public class HashCalculationRepository : IHashCalculationsRepository
    {
        private readonly Dictionary<HashType, Stack<IHashCalculation>> _hashCalculations;
        private readonly IApplicationContext _applicationContext;
        private readonly object _sync = new object();

        public HashCalculationRepository(IHashCalculation[] hashCalculations, IApplicationContext applicationContext)
        {
            _hashCalculations = new Dictionary<HashType, Stack<IHashCalculation>>();

            foreach (IHashCalculation calculation in hashCalculations)
            {
                if(!_hashCalculations.ContainsKey(calculation.HashType))
                {
                    _hashCalculations.Add(calculation.HashType, new Stack<IHashCalculation>());
                }

                _hashCalculations[calculation.HashType].Push(calculation);
            }

            _applicationContext = applicationContext;
        }

        public IHashCalculation Create(HashType key)
        {
            if (!_hashCalculations.ContainsKey(key))
            {
                throw new HashAlgorithmNotSupportedException(key);
            }

            lock (_sync)
            {
                if (_hashCalculations[key].Count > 1)
                {
                    return _hashCalculations[key].Pop();
                }

                IHashCalculation calculationTemp = _hashCalculations[key].Pop();
                IHashCalculation calculation = (IHashCalculation)_applicationContext.Container.Resolve(calculationTemp.GetType());
                _hashCalculations[key].Push(calculationTemp);
                return calculation;
            }
        }

        public void Utilize(IHashCalculation obj)
        {
            if (!_hashCalculations.ContainsKey(obj.HashType))
            {
                throw new HashAlgorithmNotSupportedException(obj.HashType);
            }

            _hashCalculations[obj.HashType].Push(obj);
        }
    }
}
