using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class NotSupportedPacketTypeHandlerException : Exception
    {
        public NotSupportedPacketTypeHandlerException() { }
        public NotSupportedPacketTypeHandlerException(PacketType packetType) : base(string.Format(Resources.ERR_NOT_SUPPORTED_PACKET_TYPE_HANDLER, packetType)) { }
        public NotSupportedPacketTypeHandlerException(PacketType packetType, Exception inner) : base(string.Format(Resources.ERR_NOT_SUPPORTED_PACKET_TYPE_HANDLER, packetType), inner) { }
        protected NotSupportedPacketTypeHandlerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
