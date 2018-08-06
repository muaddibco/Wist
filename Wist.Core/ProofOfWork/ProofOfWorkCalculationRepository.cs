using Unity;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.ProofOfWork
{
    [RegisterDefaultImplementation(typeof(IProofOfWorkCalculationRepository), Lifetime = LifetimeManagement.Singleton)]
    public class ProofOfWorkCalculationRepository : IProofOfWorkCalculationRepository
    {
        private readonly Dictionary<POWType, Stack<IProofOfWorkCalculation>> _proofOfWorkCalculations;
        private readonly IApplicationContext _applicationContext;
        private readonly object _sync = new object();

        public ProofOfWorkCalculationRepository(IProofOfWorkCalculation[] proofOfWorkCalculations, IApplicationContext applicationContext)
        {
            _proofOfWorkCalculations = new Dictionary<POWType, Stack<IProofOfWorkCalculation>>();

            foreach (IProofOfWorkCalculation calculation in proofOfWorkCalculations)
            {
                if(!_proofOfWorkCalculations.ContainsKey(calculation.POWType))
                {
                    _proofOfWorkCalculations.Add(calculation.POWType, new Stack<IProofOfWorkCalculation>());
                }

                _proofOfWorkCalculations[calculation.POWType].Push(calculation);
            }

            _applicationContext = applicationContext;
        }

        public IProofOfWorkCalculation Create(POWType key)
        {
            if (!_proofOfWorkCalculations.ContainsKey(key))
            {
                throw new ProofOfWorkAlgorithmNotSupportedException(key);
            }

            lock (_sync)
            {
                if (_proofOfWorkCalculations[key].Count > 1)
                {
                    return _proofOfWorkCalculations[key].Pop();
                }

                IProofOfWorkCalculation calculationTemp = _proofOfWorkCalculations[key].Pop();
                IProofOfWorkCalculation calculation = (IProofOfWorkCalculation)_applicationContext.Container.Resolve(calculationTemp.GetType());
                _proofOfWorkCalculations[key].Push(calculationTemp);
                return calculation;
            }
        }

        public void Utilize(IProofOfWorkCalculation obj)
        {
            if (!_proofOfWorkCalculations.ContainsKey(obj.POWType))
            {
                throw new ProofOfWorkAlgorithmNotSupportedException(obj.POWType);
            }

            _proofOfWorkCalculations[obj.POWType].Push(obj);
        }
    }
}
