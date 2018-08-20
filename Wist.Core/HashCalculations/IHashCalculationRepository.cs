using Wist.Core.Architecture;

namespace Wist.Core.HashCalculations
{
    [ServiceContract]
    public interface IHashCalculationRepository : IFactory<IHashCalculation, HashType>
    {
    }
}
