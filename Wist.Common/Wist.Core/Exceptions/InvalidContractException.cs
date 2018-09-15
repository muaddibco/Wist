using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Exceptions
{
    [Serializable]
    public class InvalidContractException : Exception
    {
        public InvalidContractException() { }
        public InvalidContractException(string message) : base(message) { }
        public InvalidContractException(string message, Exception inner) : base(message, inner) { }
        protected InvalidContractException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
