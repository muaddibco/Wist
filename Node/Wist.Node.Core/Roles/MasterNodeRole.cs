using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    [RegisterExtension(typeof(IRole), Lifetime = LifetimeManagement.Singleton)]
    public class MasterNodeRole : RoleBase
    {
        private readonly ICommunicationService _transactionsCommunicationService;
        private readonly ICommunicationService _consensusCommunicationService;

        public MasterNodeRole(ICommunicationServicesFactory communicationServicesFactory, ILoggerService loggerService) : base(loggerService)
        {
            _transactionsCommunicationService = communicationServicesFactory.Create("UdpCommunicationService");
            _consensusCommunicationService = communicationServicesFactory.Create("TcpIntermittentCommunicationService");
        }

        public override string Name => nameof(MasterNodeRole);

        protected override void InitializeInner()
        {
            SocketListenerSettings udpSettings = new SocketListenerSettings(1, 1024, new IPEndPoint(IPAddress.Any, 5023));
            SocketListenerSettings tcpSettings = new SocketListenerSettings(600, 65535, new IPEndPoint(IPAddress.Any, 5022));

            _transactionsCommunicationService.Init(udpSettings, null);
            _consensusCommunicationService.Init(tcpSettings, null);
        }

        public override Task Play()
        {
            return Task.Run(() => 
            {
                _transactionsCommunicationService.Start();
                _consensusCommunicationService.Start();
            });
        }
    }
}
