using CommonServiceLocator;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Logging;
using Wist.Node.Core.Configuration;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Roles;
using Wist.Simulation.Load.Configuration;

namespace Wist.Simulation.Load
{

    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SimpleLoadModule : ModuleBase
    {
        private ICommunicationChannel _communicationChannel;
        private readonly ICommunicationService _communicationService;
        private readonly IConfigurationService _configurationService;

        public SimpleLoadModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService) : base(loggerService)
        {
            _communicationService = clientCommunicationServiceRepository.GetInstance(nameof(TcpClientCommunicationService));
            _configurationService = configurationService;
        }

        public override string Name => nameof(SimpleLoadModule);

        protected override void InitializeInner()
        {
            ClientTcpCommunicationConfiguration clientTcpCommunicationConfiguration = _configurationService.Get<ClientTcpCommunicationConfiguration>();
            _communicationService.Init(new SocketSettings(clientTcpCommunicationConfiguration.MaxConnections, clientTcpCommunicationConfiguration.ReceiveBufferSize, clientTcpCommunicationConfiguration.ListeningPort, System.Net.Sockets.AddressFamily.InterNetwork));
        }
    }
}
