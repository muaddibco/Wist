using System.Collections.Generic;
using System.Linq;
using System.Net;
using Wist.BlockLattice.Core.DataModel.Nodes;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.SQLite.DataAccess;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.SQLite.DataServices
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

        public Node Get(IKey key)
        {
            DataModel.Node node = DataAccessService.Instance.GetNode(key);

            if(node != null)
            {
                return new Node { Key = key, IPAddress = new IPAddress(node.IPAddress)};
            }

            return null;
        }

        public IEnumerable<Node> GetAll()
        {
            return DataAccessService.Instance.GetAllNodes().Select(n => new Node { Key = _identityKeyProvider.GetKey(n.Identity.PublicKey), IPAddress = new IPAddress(n.IPAddress)});
        }

        public void Initialize()
        {
        }

        public void Update(IKey key, Node item)
        {
            DataAccessService.Instance.UpdateNode(key, item.IPAddress);
        }
    }
}
