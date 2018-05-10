using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IChainValidationServiceManager
    {
        IChainValidationService GetChainValidationService(ChainType chainType);
    }
}
