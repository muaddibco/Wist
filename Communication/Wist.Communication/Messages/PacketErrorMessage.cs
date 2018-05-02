using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Communication.Messages
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
