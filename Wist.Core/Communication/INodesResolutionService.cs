using System.Collections.Generic;
using System.Net;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Core.Communication
{
    [ServiceContract]
    public interface INodesResolutionService
    {
        void Initialize();

        void UpdateSingleNode(NodeAddress nodeAddress);

        void UpdateBulkNodes(IEnumerable<NodeAddress> nodeAddresses);

        IPAddress ResolveNodeAddress(IKey key);
    }
}
