using System.Collections.Generic;
using System.Linq;
using System.Net;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.DataModel.Nodes;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.SQLite.DataAccess;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.SQLite.DataServices
{

    [RegisterDefaultImplementation(typeof(INodesDataService), Lifetime = LifetimeManagement.Singleton)]
    public class NodesDataService : INodesDataService
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public NodesDataService(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public void Add(Node item)
        {
            DataAccessService.Instance.AddNode(item.Key, item.NodeRole, item.IPAddress);
        }

        public Node Get(UniqueKey key)
        {
            DataModel.NodeRecord node = DataAccessService.Instance.GetNode(key.IdentityKey);

            if(node != null)
            {
                return new Node { Key = key.IdentityKey, IPAddress = new IPAddress(node.IPAddress)};
            }

            return null;
        }

        public IEnumerable<Node> GetAll()
        {
            return DataAccessService.Instance.GetAllNodes().Select(n => new Node { Key = _identityKeyProvider.GetKey(n.Identity.PublicKey), IPAddress = new IPAddress(n.IPAddress), NodeRole = (NodeRole)n.NodeRole });
        }

        public IEnumerable<Node> GetAll(UniqueKey key)
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
        }

        public void Update(UniqueKey key, Node item)
        {
            DataAccessService.Instance.UpdateNode(key.IdentityKey, item.IPAddress);
        }
    }
}
