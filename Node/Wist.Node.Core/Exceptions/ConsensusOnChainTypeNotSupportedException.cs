using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Node.Core.Properties;

namespace Wist.Node.Core.Exceptions
{

    [Serializable]
    public class ConsensusOnChainTypeNotSupportedException : Exception
    {
        public ConsensusOnChainTypeNotSupportedException() { }
        public ConsensusOnChainTypeNotSupportedException(PacketType chainType) : base(string.Format(Resources.ERR_CONSENSUS_ON_CHAINTYPE_NOT_SUPPORTED, chainType)) { }
        public ConsensusOnChainTypeNotSupportedException(PacketType chainType, Exception inner) : base(string.Format(Resources.ERR_CONSENSUS_ON_CHAINTYPE_NOT_SUPPORTED, chainType), inner) { }
        protected ConsensusOnChainTypeNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
