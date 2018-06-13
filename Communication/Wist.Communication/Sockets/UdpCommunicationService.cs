using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;

namespace Wist.Communication.Sockets
{
    [RegisterExtension(typeof(ICommunicationService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class UdpCommunicationService : CommunicationServiceBase
    {
        public UdpCommunicationService(IBufferManager bufferManager, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) : base(bufferManager, packetsHandler, nodesResolutionService)
        {
        }

        public override string Name => nameof(UdpCommunicationService);

        protected override void StartAccept()
        {
            InitializeCommunicationChannel(_listenSocket);
        }

        protected override Socket CreateSocket()
        {
            return new Socket(_settings.ListeningEndpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
        }
    }
}
