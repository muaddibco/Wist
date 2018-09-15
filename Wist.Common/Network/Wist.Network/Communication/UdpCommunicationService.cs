using System.Net.Sockets;
using Wist.BlockLattice.Core.Handlers;
using Wist.Network.Interfaces;
using Wist.Network.Topology;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;

namespace Wist.Network.Communication
{
    [RegisterExtension(typeof(IServerCommunicationService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class UdpCommunicationService : ServerCommunicationServiceBase
    {
        public UdpCommunicationService(IApplicationContext applicationContext, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) 
            : base(applicationContext, loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
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
