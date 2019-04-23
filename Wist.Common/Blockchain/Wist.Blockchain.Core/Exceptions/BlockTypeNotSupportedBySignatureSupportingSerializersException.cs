using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Properties;

namespace Wist.Blockchain.Core.Exceptions
{

    [Serializable]
    public class BlockTypeNotSupportedBySignatureSupportingSerializersException : Exception
    {
        public BlockTypeNotSupportedBySignatureSupportingSerializersException() { }
        public BlockTypeNotSupportedBySignatureSupportingSerializersException(PacketType chainType, ushort blockType) : base(string.Format(Resources.ERR_SIGNATURE_SUPPORTING_SERIALIZERS_BLOCK_TYPE_NOT_SUPPORTED, blockType, chainType)) { }
        public BlockTypeNotSupportedBySignatureSupportingSerializersException(PacketType chainType, ushort blockType, Exception inner) : base(string.Format(Resources.ERR_SIGNATURE_SUPPORTING_SERIALIZERS_BLOCK_TYPE_NOT_SUPPORTED, blockType, chainType), inner) { }
        protected BlockTypeNotSupportedBySignatureSupportingSerializersException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
