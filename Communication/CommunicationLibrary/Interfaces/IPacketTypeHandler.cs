using CommunicationLibrary.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Wist.Core.Architecture;

namespace CommunicationLibrary.Interfaces
{
    [ServiceContract]
    public interface IPacketTypeHandler
    {
        PacketTypes PacketType { get; }

        Task<PacketErrorMessage> ProcessPacket(byte[] packet);
    }
}
