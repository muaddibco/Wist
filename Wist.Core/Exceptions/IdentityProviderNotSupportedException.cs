using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class IdentityProviderNotSupportedException : Exception
    {
        public IdentityProviderNotSupportedException() { }
        public IdentityProviderNotSupportedException(string identityProviderName) : base(string.Format(Resources.ERR_IDENTITY_PROVIDER_NOT_SUPPORTED, identityProviderName)) { }
        public IdentityProviderNotSupportedException(string identityProviderName, Exception inner) : base(string.Format(Resources.ERR_IDENTITY_PROVIDER_NOT_SUPPORTED, identityProviderName), inner) { }
        protected IdentityProviderNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
