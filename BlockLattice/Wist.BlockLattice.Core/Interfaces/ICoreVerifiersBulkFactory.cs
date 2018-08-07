using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Interfaces
{
    [ServiceContract]
    public interface ICoreVerifiersBulkFactory : IBulkFactory<ICoreVerifier>
    {
    }
}
