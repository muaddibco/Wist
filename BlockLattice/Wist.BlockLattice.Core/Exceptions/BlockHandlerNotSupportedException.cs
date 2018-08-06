using System;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class BlockHandlerNotSupportedException : Exception
    {
        public BlockHandlerNotSupportedException() { }
        public BlockHandlerNotSupportedException(PacketType packetType) : base(string.Format(Resources.ERR_BLOCK_HANDLER_NOT_SUPPORTED, packetType)) { }
        public BlockHandlerNotSupportedException(PacketType packetType, Exception inner) : base(string.Format(Resources.ERR_BLOCK_HANDLER_NOT_SUPPORTED, packetType), inner) { }
        protected BlockHandlerNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
