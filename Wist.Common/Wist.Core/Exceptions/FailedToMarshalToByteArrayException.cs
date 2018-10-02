using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class FailedToMarshalToByteArrayException : Exception
    {
        public FailedToMarshalToByteArrayException() { }
        public FailedToMarshalToByteArrayException(string argName) : base(string.Format(Resources.ERR_FAILED_TO_MARCHAL_TO_BYTE_ARRAY, argName)) { }
        public FailedToMarshalToByteArrayException(string argName, Exception inner) : base(string.Format(Resources.ERR_FAILED_TO_MARCHAL_TO_BYTE_ARRAY, argName), inner) { }
        protected FailedToMarshalToByteArrayException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
