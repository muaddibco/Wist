using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.DataModel.Nodes;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.Core.Serializers;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Identity;

namespace Wist.Network.Topology
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
            if(_nodeAddresses.ContainsKey(key))
            {
                return _nodeAddresses[key].IP;
            }

            Node node = _nodesDataService.Get(new UniqueKey(key));

            if(node != null)
            {
                _nodeAddresses.AddOrUpdate(key, new NodeAddress(key, node.IPAddress), (k, v) => v);
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
            //TODO: need to understand logic of distribution of transactions between storage nodes
            //IKey key = transactionKeyProvider.GetKey();

            //TODO: implement logic of recognizing storage nodes basing on MurMur Hash value of transaction content
            return _storageLayerNodeAddresses.Keys;
        }
    }
}
