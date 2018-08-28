using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Synchronization;

namespace Wist.Node.Core.Roles
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducerModule : ModuleBase
    {
        private readonly ISynchronizationGroupParticipationService _synchronizationGroupParticipationService;
        private readonly IBlocksHandlersRegistry _blocksHandlersFactory;

        public SynchronizationProducerModule(ILoggerService loggerService, ISynchronizationGroupParticipationService synchronizationGroupParticipationService, IBlocksHandlersRegistry blocksHandlersFactory) : base(loggerService)
        {
            _synchronizationGroupParticipationService = synchronizationGroupParticipationService;
            _blocksHandlersFactory = blocksHandlersFactory;
        }

        public override string Name => nameof(SynchronizationProducerModule);

        public override void Start()
        {
            _synchronizationGroupParticipationService.Start();
        }

        protected override void InitializeInner()
        {
            IBlocksHandler blocksHandler = _blocksHandlersFactory.GetInstance(TransactionsRegistrationDecisionHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);

            _synchronizationGroupParticipationService.Initialize();
        }
    }
}
