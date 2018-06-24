using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Identity;

namespace Wist.Node.Core
{
    [RegisterDefaultImplementation(typeof(INodesResolutionService), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class NodesResolutionService : INodesResolutionService
    {
        public void Initialize()
        {
            
        }

        public IPAddress ResolveNodeAddress(IKey key)
        {
            return IPAddress.Any;
        }
    }
}
