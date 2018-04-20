using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class MandatoryInterfaceNotImplementedException : Exception
    {
        public MandatoryInterfaceNotImplementedException() { }
        public MandatoryInterfaceNotImplementedException(Type aspect, Type interfaceType, Type declaringType) : base(string.Format(Resources.ERR_MANDATORY_INTERFACE_NOT_IMPLEMENTED, aspect, interfaceType, declaringType)) { }
        public MandatoryInterfaceNotImplementedException(Type aspect, Type interfaceType, Type declaringType, Exception inner) : base(string.Format(Resources.ERR_MANDATORY_INTERFACE_NOT_IMPLEMENTED, aspect, interfaceType, declaringType), inner) { }
        protected MandatoryInterfaceNotImplementedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
