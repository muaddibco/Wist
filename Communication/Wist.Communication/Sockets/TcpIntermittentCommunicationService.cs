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
using Wist.Core.Logging;

namespace Wist.Communication.Sockets
{
    [RegisterExtension(typeof(ICommunicationService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TcpIntermittentCommunicationService : TcpCommunicationService
    {

        public TcpIntermittentCommunicationService(ILoggerService loggerService, IBufferManager bufferManager, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) : base(loggerService, bufferManager, packetsHandler, nodesResolutionService)
        {
        }

        public override string Name => nameof(TcpIntermittentCommunicationService);

        public override void Init(SocketListenerSettings settings, IBlocksProcessor blocksProcessor, ICommunicationProvisioning communicationProvisioning = null)
        {
            RegisterOnReceivedExtendedValidation(OnCommunicationChannelReceived);

            base.Init(settings, blocksProcessor, communicationProvisioning);
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