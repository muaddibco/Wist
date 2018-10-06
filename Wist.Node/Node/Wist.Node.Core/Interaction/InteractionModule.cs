using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Logging;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Common;
using Wist.Proto.Model;

namespace Wist.Node.Core.Interaction
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class InteractionModule : ModuleBase
    {
        const string _host = "0.0.0.0";

        private readonly IInteractionConfiguration _interactionConfiguration;
        private Server _grpsServer;
        private readonly ISynchronizationContext _synchronizationContext;

        public InteractionModule(ILoggerService loggerService, IConfigurationService configurationService, IStatesRepository statesRepository) : base(loggerService)
        {
            _interactionConfiguration = configurationService.Get<IInteractionConfiguration>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
        }

        public override string Name => nameof(InteractionModule);

        public override void Start()
        {
            _grpsServer.Start();
        }

        protected override void InitializeInner()
        {
            _grpsServer = new Server
            {
                Services = { SyncManager.BindService(new SyncManagerImpl(_synchronizationContext)) },
                Ports = { { _host, _interactionConfiguration.Port, ServerCredentials.Insecure } }
            };
        }
    }
}
