using System.Collections.Generic;
using System.Net;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.Serializers;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Identity;

namespace Wist.Network.Topology
{
    [ServiceContract]
    public interface INodesResolutionService
    {
        void Initialize();

        void UpdateSingleNode(NodeAddress nodeAddress);

        void UpdateBulkNodes(IEnumerable<NodeAddress> nodeAddresses);

        IPAddress ResolveNodeAddress(IKey key);

        IEnumerable<IKey> GetStorageNodeKeys(ITransactionKeyProvider transactionKeyProvider);
    }
}
