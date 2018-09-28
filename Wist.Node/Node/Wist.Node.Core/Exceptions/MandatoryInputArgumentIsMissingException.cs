using System;
using Wist.Node.Core.Properties;

namespace Wist.Node.Core.Exceptions
{

    [Serializable]
    public class MandatoryInputArgumentIsMissingException : Exception
    {
        public MandatoryInputArgumentIsMissingException() { }
        public MandatoryInputArgumentIsMissingException(string argName) : base(string.Format(Resources.ERR_MANDATORY_INPUT_ARGUMENT_MISSING, argName)) { }
        public MandatoryInputArgumentIsMissingException(string argName, Exception inner) : base(string.Format(Resources.ERR_MANDATORY_INPUT_ARGUMENT_MISSING, argName), inner) { }
        protected MandatoryInputArgumentIsMissingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
