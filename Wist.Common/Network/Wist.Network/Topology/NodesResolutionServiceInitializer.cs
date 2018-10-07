using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Network.Topology
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class NodesResolutionServiceInitializer : InitializerBase
    {
        private readonly INodesResolutionService _nodesResolutionService;

        public NodesResolutionServiceInitializer(INodesResolutionService nodesResolutionService)
        {
            _nodesResolutionService = nodesResolutionService;
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override void InitializeInner()
        {
            _nodesResolutionService.Initialize();
        }
    }
}
