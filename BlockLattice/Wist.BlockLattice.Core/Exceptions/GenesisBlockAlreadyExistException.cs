using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class GenesisBlockAlreadyExistException : Exception
    {
        public GenesisBlockAlreadyExistException() { }
        public GenesisBlockAlreadyExistException(string hashValue) : base(string.Format(Resources.ERR_GENESIS_BLOCK_ALREADY_EXISTS, hashValue)) { }
        public GenesisBlockAlreadyExistException(string hashValue, Exception inner) : base(string.Format(Resources.ERR_GENESIS_BLOCK_ALREADY_EXISTS, hashValue), inner) { }
        protected GenesisBlockAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
