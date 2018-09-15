using System;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class PredicateIsNotSupportedException : Exception
    {
        public PredicateIsNotSupportedException() { }
        public PredicateIsNotSupportedException(string name) : base(string.Format(Resources.ERR_PREDICATE_IS_NOT_SUPPORTED, name)) { }
        public PredicateIsNotSupportedException(string name, Exception inner) : base(string.Format(Resources.ERR_PREDICATE_IS_NOT_SUPPORTED, name), inner) { }
        protected PredicateIsNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
