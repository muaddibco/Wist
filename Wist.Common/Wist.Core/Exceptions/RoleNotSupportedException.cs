using System;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class RoleNotSupportedException : Exception
    {
        public RoleNotSupportedException() { }
        public RoleNotSupportedException(string roleName) : base(string.Format(Resources.ERR_ROLE_NOT_SUPPORTED, roleName)) { }
        public RoleNotSupportedException(string roleName, Exception inner) : base(string.Format(Resources.ERR_ROLE_NOT_SUPPORTED, roleName), inner) { }
        protected RoleNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
