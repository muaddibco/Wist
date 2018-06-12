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
    public class MasterNodeRole : IRole
    {
        private readonly ICommunicationService _transactionsCommunicationService;
        private readonly ICommunicationService _consensusCommunicationService;

        public MasterNodeRole(ICommunicationServicesFactory communicationServicesFactory)
        {
            _transactionsCommunicationService = communicationServicesFactory.Create("UdpCommunicationService");
            _consensusCommunicationService = communicationServicesFactory.Create("TcpIntermittentCommunicationService");
        }

        public string Name => nameof(MasterNodeRole);

        public void Initialize()
        {
            SocketListenerSettings udpSettings = new SocketListenerSettings(1, 1024, new IPEndPoint(IPAddress.Any, 5023));
            SocketListenerSettings tcpSettings = new SocketListenerSettings(600, 65535, new IPEndPoint(IPAddress.Any, 5022));

            _transactionsCommunicationService.Init(udpSettings, null);
            _consensusCommunicationService.Init(tcpSettings, null);
        }

        public Task Play()
        {
            return Task.Run(() => 
            {
                _transactionsCommunicationService.Start();
                _consensusCommunicationService.Start();
            });
        }
    }
}
