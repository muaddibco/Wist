using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Identity;

namespace Wist.Communication.Topology
{
    [RegisterDefaultImplementation(typeof(INodesResolutionService), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class NodesResolutionService : INodesResolutionService
    {
        private readonly INodesDataService _nodesDataService;
        private readonly ConcurrentDictionary<IKey, NodeAddress> _nodeAddresses;

        public NodesResolutionService(INodesDataService nodesDataService)
        {
            _nodesDataService = nodesDataService;
            _nodeAddresses = new ConcurrentDictionary<IKey, NodeAddress>();
        }

        public void Initialize()
        {
            IEnumerable<BlockLattice.Core.DataModel.Nodes.Node> nodes = _nodesDataService.GetAll();

            foreach (var node in nodes)
            {
                _nodeAddresses.AddOrUpdate(node.Key, new NodeAddress(node.Key, node.IPAddress), (k, v) => v);
            }
        }

        public IPAddress ResolveNodeAddress(IKey key)
        {
            BlockLattice.Core.DataModel.Nodes.Node node = _nodesDataService.Get(key);

            if(node != null)
            {
                return node.IPAddress;
            }

            return IPAddress.None;
        }

        public void UpdateBulkNodes(IEnumerable<NodeAddress> nodeAddresses)
        {
            foreach (var nodeAddress in nodeAddresses)
            {
                UpdateSingleNode(nodeAddress);
            }
        }

        public void UpdateSingleNode(NodeAddress nodeAddress)
        {
            _nodeAddresses.AddOrUpdate(nodeAddress.Key, nodeAddress, (k, v) => nodeAddress);
        }
    }
}
