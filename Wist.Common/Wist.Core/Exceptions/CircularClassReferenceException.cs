using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class CircularClassReferenceException : Exception
    {
        public CircularClassReferenceException() { }
        public CircularClassReferenceException(Type type1, Type type2) : base(string.Format(Resources.ERR_CIRCULAR_CLASS_REFERENCE, type1.FullName, type2.FullName)) { }
        public CircularClassReferenceException(Type type1, Type type2, Exception inner) : base(string.Format(Resources.ERR_CIRCULAR_CLASS_REFERENCE, type1.FullName, type2.FullName), inner) { }
        protected CircularClassReferenceException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
