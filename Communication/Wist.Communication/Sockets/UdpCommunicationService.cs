using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;
using Wist.Core.Logging;

namespace Wist.Communication.Sockets
{
    [RegisterExtension(typeof(IServerCommunicationService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class UdpCommunicationService : ServerCommunicationServiceBase
    {
        public UdpCommunicationService(ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) : base(loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
        {
        }

        public override string Name => "GenericUdp";

        protected override void StartAccept()
        {
            InitializeCommunicationChannel(_listenSocket);
        }

        protected override Socket CreateSocket()
        {
            return new Socket(_settings.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }
    }
}
