using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class ChainTypeNotSupportedBySignatureSupportingSerializersException : Exception
    {
        public ChainTypeNotSupportedBySignatureSupportingSerializersException() { }
        public ChainTypeNotSupportedBySignatureSupportingSerializersException(PacketType chainType) : base(string.Format(Resources.ERR_SIGNATURE_SUPPORTING_SERIALIZERS_CHAIN_TYPE_NOT_SUPPORTED, chainType)) { }
        public ChainTypeNotSupportedBySignatureSupportingSerializersException(PacketType chainType, Exception inner) : base(string.Format(Resources.ERR_SIGNATURE_SUPPORTING_SERIALIZERS_CHAIN_TYPE_NOT_SUPPORTED, chainType), inner) { }
        protected ChainTypeNotSupportedBySignatureSupportingSerializersException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
