using Wist.Core.Architecture;

namespace Wist.Core.HashCalculations
{
    [ServiceContract]
    public interface IHashCalculationsRepository : IFactory<IHashCalculation, HashType>
    {
    }
}
