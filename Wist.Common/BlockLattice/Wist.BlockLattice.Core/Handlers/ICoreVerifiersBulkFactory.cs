using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Handlers
{
    [ServiceContract]
    public interface ICoreVerifiersBulkFactory : IBulkFactory<ICoreVerifier>
    {
    }
}
