using Wist.Core;
using Wist.Core.Architecture;

namespace Wist.Blockchain.Core.Handlers
{
    [ServiceContract]
    public interface ICoreVerifiersBulkFactory : IBulkFactory<ICoreVerifier>
    {
    }
}
