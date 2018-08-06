using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Roles;
using Wist.Node.Core.Synchronization;

namespace Wist.Node.Core
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class CommonModule : ModuleBase
    {
        public const string NAME = nameof(CommonModule);
        private readonly IBlocksHandlersFactory _blocksHandlersFactory;

        public CommonModule(ILoggerService loggerService, IBlocksHandlersFactory blocksHandlersFactory) : base(loggerService)
        {
            _blocksHandlersFactory = blocksHandlersFactory;
        }

        public override string Name => NAME;

        protected override void InitializeInner()
        {
            IBlocksHandler blocksHandler = _blocksHandlersFactory.GetInstance(SynchronizationReceivingHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);
        }
    }
}
