using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace CommunicationLibrary.Messages.PacketTypeHandlers
{
    [RegisterExtension(typeof(IPacketTypeHandler), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class HashBasedPacketTypeHandler : IPacketTypeHandler
    {
        public PacketTypes PacketType => PacketTypes.HashBased;

        public async Task<PacketErrorMessage> ProcessPacket(byte[] packet)
        {
            return await Task.FromResult(new PacketErrorMessage(PacketsErrors.NO_ERROR));
        }
    }
}
