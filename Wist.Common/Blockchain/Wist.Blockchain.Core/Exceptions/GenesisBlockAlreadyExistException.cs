using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.Properties;

namespace Wist.Blockchain.Core.Exceptions
{

    [Serializable]
    public class GenesisBlockAlreadyExistException : Exception
    {
        public GenesisBlockAlreadyExistException() { }
        public GenesisBlockAlreadyExistException(string keyValue) : base(string.Format(Resources.ERR_IDENTITY_ALREADY_EXISTS, keyValue)) { }
        public GenesisBlockAlreadyExistException(string keyValue, Exception inner) : base(string.Format(Resources.ERR_IDENTITY_ALREADY_EXISTS, keyValue), inner) { }
        protected GenesisBlockAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
