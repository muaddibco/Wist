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
    public class FullNodeRole : RoleBase
    {
        private readonly ICommunicationService _transactionsCommunicationService;

        public FullNodeRole(ICommunicationServicesFactory communicationServicesFactory, ILoggerService loggerService) : base(loggerService)
        {
            _transactionsCommunicationService = communicationServicesFactory.Create("TcpIntermittentCommunicationService");
        }

        public override string Name => nameof(FullNodeRole);

        protected override void InitializeInner()
        {
            SocketListenerSettings tcpSettings = new SocketListenerSettings(100, 65535, new IPEndPoint(IPAddress.Any, 5024));

            _transactionsCommunicationService.Init(tcpSettings, null);
        }

        public override Task Play()
        {
            return Task.Run(() => { _transactionsCommunicationService.Start(); });
        }
    }
}
