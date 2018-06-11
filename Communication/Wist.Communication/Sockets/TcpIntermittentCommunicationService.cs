using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Communication;

namespace Wist.Communication.Sockets
{
    [RegisterExtension(typeof(ICommunicationService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TcpIntermittentCommunicationService : TcpPersistentCommunicationService
    {

        public TcpIntermittentCommunicationService(IBufferManager bufferManager, IPacketSerializersFactory packetSerializersFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) : base(bufferManager, packetSerializersFactory, packetsHandler, nodesResolutionService)
        {
        }

        public override string Name => nameof(TcpIntermittentCommunicationService);

        protected override void OnCommunicationChannelReceived(ICommunicationChannel communicationChannel, int receivedBytes)
        {
            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (receivedBytes == 0)
            {
                _log.Info($"ProcessReceive NO DATA from IP {communicationChannel.RemoteIPAddress}");

                communicationChannel.Close();

                return;
            }
        }
    }
}