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

        public SynchronizationProducerModule(ILoggerService loggerService, ISynchronizationGroupParticipationService synchronizationGroupParticipationService) : base(loggerService)
        {
            _synchronizationGroupParticipationService = synchronizationGroupParticipationService;
        }

        public override string Name => nameof(SynchronizationProducerModule);

        public override void Start()
        {
            _synchronizationGroupParticipationService.Start();
        }

        protected override void InitializeInner()
        {
            _synchronizationGroupParticipationService.Initialize();
        }
    }
}
