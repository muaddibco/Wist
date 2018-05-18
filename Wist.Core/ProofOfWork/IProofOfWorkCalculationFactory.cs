using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.ProofOfWork
{
    [ServiceContract]
    public interface IProofOfWorkCalculationFactory : IFactory<IProofOfWorkCalculation, POWType>
    {
    }
}
