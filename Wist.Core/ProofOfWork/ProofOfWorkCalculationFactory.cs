using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.ProofOfWork
{
    [RegisterDefaultImplementation(typeof(IProofOfWorkCalculationFactory), Lifetime = LifetimeManagement.Singleton)]
    public class ProofOfWorkCalculationFactory : IProofOfWorkCalculationFactory
    {
        private readonly Dictionary<POWType, Stack<IProofOfWorkCalculation>> _proofOfWorkCalculationPool;

        public ProofOfWorkCalculationFactory(IProofOfWorkCalculation[] proofOfWorkCalculations)
        {
            _proofOfWorkCalculationPool = new Dictionary<POWType, Stack<IProofOfWorkCalculation>>();

            foreach (IProofOfWorkCalculation calculation in proofOfWorkCalculations)
            {
                if(!_proofOfWorkCalculationPool.ContainsKey(calculation.POWType))
                {
                    _proofOfWorkCalculationPool.Add(calculation.POWType, new Stack<IProofOfWorkCalculation>());
                }

                _proofOfWorkCalculationPool[calculation.POWType].Push(calculation);
            }
        }

        public IProofOfWorkCalculation Create(POWType key)
        {
            if(!_proofOfWorkCalculationPool.ContainsKey(key))
            {
                throw new ProofOfWorkAlgorithmNotSupportedException(key);
            }

            if(_proofOfWorkCalculationPool[key].Count > 1)
            {
                return _proofOfWorkCalculationPool[key].Pop();
            }
            else
            {
                IProofOfWorkCalculation calculationTemp = _proofOfWorkCalculationPool[key].Pop();
                IProofOfWorkCalculation calculation = (IProofOfWorkCalculation)ServiceLocator.Current.GetInstance(calculationTemp.GetType());
                _proofOfWorkCalculationPool[key].Push(calculationTemp);

                return calculation;
            }
        }

        public void Utilize(IProofOfWorkCalculation calculation)
        {
            if (!_proofOfWorkCalculationPool.ContainsKey(calculation.POWType))
            {
                throw new ProofOfWorkAlgorithmNotSupportedException(calculation.POWType);
            }

            _proofOfWorkCalculationPool[calculation.POWType].Push(calculation);
        }
    }
}
