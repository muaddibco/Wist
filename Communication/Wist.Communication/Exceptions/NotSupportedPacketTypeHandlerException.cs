using Wist.Communication.Properties;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Communication.Exceptions
{

    [Serializable]
    public class NotSupportedPacketTypeHandlerException : Exception
    {
        public NotSupportedPacketTypeHandlerException() { }
        public NotSupportedPacketTypeHandlerException(PacketTypes packetType) : base(string.Format(Resources.ERR_NOT_SUPPORTED_PACKET_TYPE_HANDLER, packetType)) { }
        public NotSupportedPacketTypeHandlerException(PacketTypes packetType, Exception inner) : base(string.Format(Resources.ERR_NOT_SUPPORTED_PACKET_TYPE_HANDLER, packetType), inner) { }
        protected NotSupportedPacketTypeHandlerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
