using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    public class FullNodeRole : IRole
    {
        private readonly ICommunicationService _transactionsCommunicationService;

        public FullNodeRole(ICommunicationServicesFactory communicationServicesFactory)
        {
            _transactionsCommunicationService = communicationServicesFactory.Create("TcpIntermittentCommunicationService");
        }

        public string Name => nameof(FullNodeRole);

        public void Initialize()
        {
            SocketListenerSettings tcpSettings = new SocketListenerSettings(100, 65535, new IPEndPoint(IPAddress.Any, 5024));

            _transactionsCommunicationService.Init(tcpSettings, null);
        }

        public Task Play()
        {
            return Task.Run(() => { _transactionsCommunicationService.Start(); });
        }
    }
}
