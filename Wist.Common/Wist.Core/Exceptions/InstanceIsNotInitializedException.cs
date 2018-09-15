using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class InstanceIsNotInitializedException : Exception
    {
        public InstanceIsNotInitializedException() { }
        public InstanceIsNotInitializedException(Type classType) : base(string.Format(Resources.ERR_INSTANCE_IS_NOT_INITIALIZED, classType.FullName)) { }
        public InstanceIsNotInitializedException(Type classType, Exception inner) : base(string.Format(Resources.ERR_INSTANCE_IS_NOT_INITIALIZED, classType.FullName), inner) { }
        protected InstanceIsNotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
