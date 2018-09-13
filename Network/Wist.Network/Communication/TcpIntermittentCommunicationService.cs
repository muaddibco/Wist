using System.Net;
using Wist.BlockLattice.Core.Handlers;
using Wist.Network.Interfaces;
using Wist.Network.Topology;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Logging;

namespace Wist.Network.Communication
{
    /// <summary>
    /// Seems such type of Communication Service will be used at Storage Level only
    /// </summary>
    [RegisterExtension(typeof(IServerCommunicationService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TcpIntermittentCommunicationService : TcpCommunicationService
    {

        public TcpIntermittentCommunicationService(IApplicationContext applicationContext, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) 
            : base(applicationContext, loggerService, bufferManagerFactory, packetsHandler, nodesResolutionService)
        {
        }

        public override string Name => nameof(TcpIntermittentCommunicationService);

        public override void Init(SocketSettings settings)
        {
            RegisterOnReceivedExtendedValidation(OnCommunicationChannelReceived);

            base.Init(settings);
        }

        private bool OnCommunicationChannelReceived(ICommunicationChannel communicationChannel, IPEndPoint remoteEndPoint, int receivedBytes)
        {
            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (receivedBytes == 0)
            {
                _log.Info($"ProcessReceive NO DATA from IP {communicationChannel.RemoteIPAddress}");

                communicationChannel.Close();

                return false;
            }

            return true;
        }
    }
}