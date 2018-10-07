using System.Collections.Generic;
using System.Linq;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.States;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationTopologyInitializer : InitializerBase
    {
        private readonly ISynchronizationGroupState _synchronizationGroupState;
        private readonly ISyncRegistryNeighborhoodState _syncRegistryNeighborhoodState;
        private readonly INodesDataService _nodesDataService;
        private readonly ILogger _logger;

        public SynchronizationTopologyInitializer(IStatesRepository statesRepository, INodesDataService nodesDataService, ILoggerService loggerService)
        {
            _synchronizationGroupState = statesRepository.GetInstance<ISynchronizationGroupState>();
            _syncRegistryNeighborhoodState = statesRepository.GetInstance<ISyncRegistryNeighborhoodState>();
            _nodesDataService = nodesDataService;
            _logger = loggerService.GetLogger(nameof(SynchronizationTopologyInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override void InitializeInner()
        {
            IEnumerable<BlockLattice.Core.DataModel.Nodes.Node> syncNodes = _nodesDataService.GetAll().Where(n => n.NodeRole == BlockLattice.Core.DataModel.Nodes.NodeRole.SynchronizationLayer);
            foreach (var node in syncNodes)
            {
                _synchronizationGroupState.AddNeighbor(node.Key);
            }

            IEnumerable<BlockLattice.Core.DataModel.Nodes.Node> registryNodes = _nodesDataService.GetAll().Where(n => n.NodeRole == BlockLattice.Core.DataModel.Nodes.NodeRole.TransactionsRegistrationLayer);

            foreach (var node in registryNodes)
            {
                _syncRegistryNeighborhoodState.AddNeighbor(node.Key);
            }
        }
    }
}
