using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Nodes;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers;
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
        private readonly ConcurrentDictionary<IKey, NodeAddress> _storageLayerNodeAddresses;

        public NodesResolutionService(INodesDataService nodesDataService)
        {
            _nodesDataService = nodesDataService;
            _nodeAddresses = new ConcurrentDictionary<IKey, NodeAddress>();
            _storageLayerNodeAddresses = new ConcurrentDictionary<IKey, NodeAddress>();
        }

        public void Initialize()
        {
            IEnumerable<Node> nodes = _nodesDataService.GetAll();

            foreach (var node in nodes)
            {
                _nodeAddresses.AddOrUpdate(node.Key, new NodeAddress(node.Key, node.IPAddress), (k, v) => v);
                if(node.NodeRole == NodeRole.StorageLayer)
                {
                    _storageLayerNodeAddresses.AddOrUpdate(node.Key, new NodeAddress(node.Key, node.IPAddress), (k, v) => v);
                }
            }
        }

        public IPAddress ResolveNodeAddress(IKey key)
        {
            Node node = _nodesDataService.Get(key);

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

        public IEnumerable<IKey> GetStorageNodeKeys(ITransactionKeyProvider transactionKeyProvider)
        {
            IKey key = transactionKeyProvider.GetKey();

            //TODO: implement logic of recognizing storage nodes basing on MurMur Hash value of transaction content
            return _storageLayerNodeAddresses.Keys;
        }
    }
}
