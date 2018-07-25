using System;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class PerformanceCountersServiceNotSupportedException : Exception
    {
        public PerformanceCountersServiceNotSupportedException() { }
        public PerformanceCountersServiceNotSupportedException(string performanceCountersServiceName) : base(string.Format(Resources.ERR_PERFORMANCE_COUNTERS_SERVICE_NOT_SUPPORTED, performanceCountersServiceName)) { }
        public PerformanceCountersServiceNotSupportedException(string performanceCountersServiceName, Exception inner) : base(string.Format(Resources.ERR_PERFORMANCE_COUNTERS_SERVICE_NOT_SUPPORTED, performanceCountersServiceName), inner) { }
        protected PerformanceCountersServiceNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
