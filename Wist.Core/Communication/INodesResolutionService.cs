using System.Net;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Core.Communication
{
    [ServiceContract]
    public interface INodesResolutionService
    {
        void Initialize();

        IPAddress ResolveNodeAddress(IKey key);
    }
}
