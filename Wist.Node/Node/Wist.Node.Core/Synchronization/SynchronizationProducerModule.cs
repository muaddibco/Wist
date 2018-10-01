using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Network.Interfaces;
using Wist.Node.Core.Common;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducerModule : ModuleBase
    {
        private readonly ISynchronizationGroupParticipationService _synchronizationGroupParticipationService;
        private readonly IBlocksHandlersRegistry _blocksHandlersFactory;

        public SynchronizationProducerModule(ILoggerService loggerService, ISynchronizationGroupParticipationService synchronizationGroupParticipationService, 
            IBlocksHandlersRegistry blocksHandlersFactory) 
            : base(loggerService)
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
            _synchronizationGroupParticipationService.Initialize();

            IBlocksHandler blocksHandler = _blocksHandlersFactory.GetInstance(TransactionsRegistrySyncHandler.NAME);
            IBlocksHandler blocksHandler1 = _blocksHandlersFactory.GetInstance(SynchronizationBlocksHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(blocksHandler);
            _blocksHandlersFactory.RegisterInstance(blocksHandler1);
            blocksHandler.Initialize(_cancellationToken);
            blocksHandler1.Initialize(_cancellationToken);
        }
    }
}
