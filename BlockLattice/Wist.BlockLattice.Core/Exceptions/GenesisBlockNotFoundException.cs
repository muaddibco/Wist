using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class GenesisBlockNotFoundException : Exception
    {
        public GenesisBlockNotFoundException() { }
        public GenesisBlockNotFoundException(string hashValue) : base(string.Format(Resources.ERR_GENESIS_BLOCK_NOT_FOUND, hashValue)) { }
        public GenesisBlockNotFoundException(string hashValue, Exception inner) : base(string.Format(Resources.ERR_GENESIS_BLOCK_NOT_FOUND, hashValue), inner) { }
        protected GenesisBlockNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
