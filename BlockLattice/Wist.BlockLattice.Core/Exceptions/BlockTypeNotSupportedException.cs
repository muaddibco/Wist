using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class BlockTypeNotSupportedException : Exception
    {
        public BlockTypeNotSupportedException() { }
        public BlockTypeNotSupportedException(ushort blockType, ChainType chainType) : base(string.Format(Resources.ERR_NOT_SUPPORTED_BLOCK_TYPE, blockType, chainType)) { }
        public BlockTypeNotSupportedException(ushort blockType, ChainType chainType, Exception inner) : base(string.Format(Resources.ERR_NOT_SUPPORTED_BLOCK_TYPE, blockType, chainType), inner) { }
        protected BlockTypeNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
