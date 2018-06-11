using System;
using System.Collections.Generic;
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
        public UdpCommunicationService(IBufferManager bufferManager, IPacketSerializersFactory packetSerializersFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService) : base(bufferManager, packetSerializersFactory, packetsHandler, nodesResolutionService)
        {
        }

        public override string Name => nameof(UdpCommunicationService);

        protected override void StartAccept()
        {
            InitializeCommunicationChannel(_listenSocket);
        }
    }
}
