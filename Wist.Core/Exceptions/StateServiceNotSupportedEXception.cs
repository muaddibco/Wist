using System;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class StateServiceNotSupportedException : Exception
    {
        public StateServiceNotSupportedException() { }
        public StateServiceNotSupportedException(string stateServiceName) : base(string.Format(Resources.ERR_STATE_SERVICE_NOT_SUPPORTED, stateServiceName)) { }
        public StateServiceNotSupportedException(string stateServiceName, Exception inner) : base(string.Format(Resources.ERR_STATE_SERVICE_NOT_SUPPORTED, stateServiceName), inner) { }
        protected StateServiceNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
