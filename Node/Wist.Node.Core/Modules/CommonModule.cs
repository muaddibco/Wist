using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Core.Shards;
using Wist.Node.Core.Synchronization;

namespace Wist.Node.Core.Modules
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class CommonModule : ModuleBase
    {
        public const string NAME = nameof(CommonModule);
        private readonly IBlocksHandlersRegistry _blocksHandlersFactory;
        private readonly IShardsManager _shardsManager;

        public CommonModule(ILoggerService loggerService, IBlocksHandlersRegistry blocksHandlersFactory, IShardsManager shardsManager) : base(loggerService)
        {
            _blocksHandlersFactory = blocksHandlersFactory;
            _shardsManager = shardsManager;
        }

        public override string Name => NAME;

        public override void Start()
        {
        }

        protected override void InitializeInner()
        {
            IBlocksHandler blocksHandler = _blocksHandlersFactory.GetInstance(SynchronizationReceivingHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);

            _shardsManager.Initialize(_cancellationToken);
        }
    }
}
