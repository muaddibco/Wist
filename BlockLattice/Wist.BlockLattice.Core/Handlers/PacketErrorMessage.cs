using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.Handlers
{
    public class PacketErrorMessage
    {
        public PacketErrorMessage(PacketsErrors errorCode, byte[] messagePacket = null)
        {
            MessagePacket = messagePacket;
            ErrorCode = errorCode;
        }

        public PacketsErrors ErrorCode { get; }
        public byte[] MessagePacket { get; set; }
    }
}
