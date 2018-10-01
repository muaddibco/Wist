using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class FailedToInitializeCounterException : Exception
    {
        public FailedToInitializeCounterException() { }
        public FailedToInitializeCounterException(string counterName, string categoryName) : base(string.Format(Resources.ERR_FAILED_TO_INITIALIZE_COUNTER, counterName, categoryName)) { }
        public FailedToInitializeCounterException(string counterName, string categoryName, Exception inner) : base(string.Format(Resources.ERR_FAILED_TO_INITIALIZE_COUNTER, counterName, categoryName), inner) { }
        protected FailedToInitializeCounterException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
