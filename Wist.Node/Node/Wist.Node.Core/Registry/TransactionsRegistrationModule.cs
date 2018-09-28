using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Common;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistrationModule : ModuleBase
    {
        public const string NAME = nameof(TransactionsRegistrationModule);
        private readonly IBlocksHandlersRegistry _blocksHandlersFactory;

        public TransactionsRegistrationModule(ILoggerService loggerService, IBlocksHandlersRegistry blocksHandlersFactory) : base(loggerService)
        {
            _blocksHandlersFactory = blocksHandlersFactory;
        }

        public override string Name => NAME;

        public override void Start()
        {
        }

        protected override void InitializeInner()
        {
            IBlocksHandler blocksHandler = _blocksHandlersFactory.GetInstance(TransactionsRegistryHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);
        }
    }
}
