using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class BlocksProcessorNotRegisteredException : Exception
    {
        public BlocksProcessorNotRegisteredException() { }
        public BlocksProcessorNotRegisteredException(string blocksProcessorName) : base(string.Format(Resources.ERR_BLOCKS_PROCESSOR_NOT_REGISTERED, blocksProcessorName)) { }
        public BlocksProcessorNotRegisteredException(string blocksProcessorName, Exception inner) : base(string.Format(Resources.ERR_BLOCKS_PROCESSOR_NOT_REGISTERED, blocksProcessorName), inner) { }
        protected BlocksProcessorNotRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
