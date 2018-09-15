using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class PacketTypeNotSupportedBySignatureSupportingSerializersException : Exception
    {
        public PacketTypeNotSupportedBySignatureSupportingSerializersException() { }
        public PacketTypeNotSupportedBySignatureSupportingSerializersException(PacketType chainType) : base(string.Format(Resources.ERR_SIGNATURE_SUPPORTING_SERIALIZERS_CHAIN_TYPE_NOT_SUPPORTED, chainType)) { }
        public PacketTypeNotSupportedBySignatureSupportingSerializersException(PacketType chainType, Exception inner) : base(string.Format(Resources.ERR_SIGNATURE_SUPPORTING_SERIALIZERS_CHAIN_TYPE_NOT_SUPPORTED, chainType), inner) { }
        protected PacketTypeNotSupportedBySignatureSupportingSerializersException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
