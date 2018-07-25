using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.ProofOfWork
{
    [RegisterDefaultImplementation(typeof(IProofOfWorkCalculationRepository), Lifetime = LifetimeManagement.Singleton)]
    public class ProofOfWorkCalculationRepository : IProofOfWorkCalculationRepository
    {
        private readonly Dictionary<POWType, IProofOfWorkCalculation> _proofOfWorkCalculations;

        public ProofOfWorkCalculationRepository(IProofOfWorkCalculation[] proofOfWorkCalculations)
        {
            _proofOfWorkCalculations = new Dictionary<POWType, IProofOfWorkCalculation>();

            foreach (IProofOfWorkCalculation calculation in proofOfWorkCalculations)
            {
                if(!_proofOfWorkCalculations.ContainsKey(calculation.POWType))
                {
                    _proofOfWorkCalculations.Add(calculation.POWType, calculation);
                }
            }
        }

        public IProofOfWorkCalculation GetInstance(POWType key)
        {
            if(!_proofOfWorkCalculations.ContainsKey(key))
            {
                throw new ProofOfWorkAlgorithmNotSupportedException(key);
            }

            return _proofOfWorkCalculations[key];
        }
    }
}
