using System;
using System.Collections.Generic;
using System.Text;

namespace CommunicationLibrary.Messages
{
    public class PacketErrorMessage
    {
        public PacketErrorMessage(PacketsErrors errorCode, byte[] messagePacket = null)
        {
            MessagePacket = messagePacket;
            ErrorCode = errorCode;
        }

        public PacketsErrors ErrorCode { get; }
        public byte[] MessagePacket { get; }
    }
}
