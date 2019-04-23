using Grpc.Core;
using Wist.Blockchain.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.Modularity;
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

        private IInteractionConfiguration _interactionConfiguration;
        private Server _grpsServer;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IConfigurationService _configurationService;
        private readonly IChainDataServicesManager _chainDataServicesManager;
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IHashCalculationsRepository _hashCalculationsRepository;

        public InteractionModule(ILoggerService loggerService, IConfigurationService configurationService, IStatesRepository statesRepository, 
            IChainDataServicesManager chainDataServicesManager, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            IHashCalculationsRepository hashCalculationsRepository) 
            : base(loggerService)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _configurationService = configurationService;
            _chainDataServicesManager = chainDataServicesManager;
            _identityKeyProvidersRegistry = identityKeyProvidersRegistry;
            _hashCalculationsRepository = hashCalculationsRepository;
        }

        public override string Name => nameof(InteractionModule);

        public override void Start()
        {
            _grpsServer.Start();
        }

        protected override void InitializeInner()
        {
            _interactionConfiguration = _configurationService.Get<IInteractionConfiguration>();

            _grpsServer = new Server
            {
                Services =
                {
                    SyncManager.BindService(new SyncManagerImpl(_synchronizationContext, _chainDataServicesManager, _hashCalculationsRepository, _log)),
                    TransactionalChainManager.BindService(new TransactionalChainManagerImpl(_chainDataServicesManager, _identityKeyProvidersRegistry, _hashCalculationsRepository, _log))
                },

                Ports = { { _host, _interactionConfiguration.Port, ServerCredentials.Insecure } }
            };
        }
    }
}
