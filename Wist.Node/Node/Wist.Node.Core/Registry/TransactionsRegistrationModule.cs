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
        private readonly ITransactionsRegistryService _transactionsRegistryService;

        public TransactionsRegistrationModule(ILoggerService loggerService, IBlocksHandlersRegistry blocksHandlersFactory, ITransactionsRegistryService transactionsRegistryService) : base(loggerService)
        {
            _blocksHandlersFactory = blocksHandlersFactory;
            _transactionsRegistryService = transactionsRegistryService;
        }

        public override string Name => NAME;

        public override void Start()
        {
            _transactionsRegistryService.Start();
        }

        protected override void InitializeInner()
        {
            IBlocksHandler blocksHandler = _blocksHandlersFactory.GetInstance(TransactionsRegistryHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);

            _transactionsRegistryService.Initialize();
        }
    }
}
