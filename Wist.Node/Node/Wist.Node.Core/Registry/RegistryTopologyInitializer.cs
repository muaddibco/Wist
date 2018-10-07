using System.Collections.Generic;
using System.Linq;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.States;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryTopologyInitializer : InitializerBase
    {
        private readonly IRegistryGroupState _registryGroupState;
        private readonly INodesDataService _nodesDataService;
        private readonly ILogger _logger;

        public RegistryTopologyInitializer(IStatesRepository statesRepository, INodesDataService nodesDataService, ILoggerService loggerService)
        {
            _registryGroupState = statesRepository.GetInstance<IRegistryGroupState>();
            _nodesDataService = nodesDataService;
            _logger = loggerService.GetLogger(nameof(RegistryTopologyInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override void InitializeInner()
        {
            IEnumerable<BlockLattice.Core.DataModel.Nodes.Node> registryNodes = _nodesDataService.GetAll().Where(n => n.NodeRole == BlockLattice.Core.DataModel.Nodes.NodeRole.TransactionsRegistrationLayer);

            foreach (var node in registryNodes)
            {
                _registryGroupState.AddNeighbor(node.Key);
            }

            IEnumerable<BlockLattice.Core.DataModel.Nodes.Node> syncNodes = _nodesDataService.GetAll().Where(n => n.NodeRole == BlockLattice.Core.DataModel.Nodes.NodeRole.SynchronizationLayer);
            _registryGroupState.SyncLayerNode = syncNodes.FirstOrDefault()?.Key;
        }
    }
}
