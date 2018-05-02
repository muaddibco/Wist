using Wist.Communication.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;

namespace Wist.Communication.Interfaces
{
    [ServiceContract]
    public interface IPacketTypeHandler
    {
        PacketTypes PacketType { get; }

        Task<PacketErrorMessage> ProcessPacket(byte[] packet);
    }
}
