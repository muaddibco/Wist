using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;
using Wist.Core.Logging;
using Wist.Core.States;

namespace Wist.Node.Core.Common
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class CommonTopologyInitializer : InitializerBase
    {
        private readonly INeighborhoodState _neighborhoodState;
        private readonly INodesDataService _nodesDataService;
        private readonly ILogger _logger;

        public CommonTopologyInitializer(IStatesRepository statesRepository, INodesDataService nodesDataService, ILoggerService loggerService)
        {
            _neighborhoodState = statesRepository.GetInstance<INeighborhoodState>();
            _nodesDataService = nodesDataService;
            _logger = loggerService.GetLogger(nameof(CommonTopologyInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.AboveNormal;

        protected override void InitializeInner()
        {
            IEnumerable<BlockLattice.Core.DataModel.Nodes.Node> nodes = _nodesDataService.GetAll();
            foreach (var node in nodes)
            {
                _neighborhoodState.AddNeighbor(node.Key);
            }
        }
    }
}
