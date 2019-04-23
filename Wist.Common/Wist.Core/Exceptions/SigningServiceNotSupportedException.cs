using System;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class SigningServiceNotSupportedException : Exception
    {
        public SigningServiceNotSupportedException() { }
        public SigningServiceNotSupportedException(string signingServiceName) : base(string.Format(Resources.ERR_SIGNING_SERVICE_NOT_SUPPORTED, signingServiceName)) { }
        public SigningServiceNotSupportedException(string signingServiceName, Exception inner) : base(string.Format(Resources.ERR_SIGNING_SERVICE_NOT_SUPPORTED, signingServiceName)) { }
        protected SigningServiceNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
