using System.Net.Sockets;
using Wist.BlockLattice.Core.Handlers;
using Wist.Network.Interfaces;
using Wist.Network.Topology;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;

namespace Wist.Network.Communication
{

    [RegisterExtension(typeof(ICommunicationService), Lifetime = LifetimeManagement.Singleton)]
    public class TcpClientCommunicationService : CommunicationServiceBase
    {

        public TcpClientCommunicationService(IApplicationContext applicationContext, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService)
            : base(applicationContext, loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
        {

        }

        public override string Name => nameof(TcpClientCommunicationService);

        protected override Socket CreateSocket()
        {
            return new Socket(_settings.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}
