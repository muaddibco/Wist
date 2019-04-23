using System;
using Wist.Blockchain.Core.Properties;

namespace Wist.Blockchain.Core.Exceptions
{

    [Serializable]
    public class RawBytesNotInitializedException : Exception
    {
        public RawBytesNotInitializedException() : base(Resources.ERR_RAW_BYTES_NOT_INITIALIZED) { }
        public RawBytesNotInitializedException(Exception inner) : base(Resources.ERR_RAW_BYTES_NOT_INITIALIZED, inner) { }
        protected RawBytesNotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
