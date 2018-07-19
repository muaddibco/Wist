using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Identity;

namespace Wist.Node.Core
{
    [RegisterDefaultImplementation(typeof(INodesResolutionService), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class NodesResolutionService : INodesResolutionService
    {
        private readonly INodesDataService _nodesDataService;

        public NodesResolutionService(INodesDataService nodesDataService)
        {
            _nodesDataService = nodesDataService;
        }

        public void Initialize()
        {
            
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
    }
}
