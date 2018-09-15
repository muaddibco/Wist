using System;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{

    [Serializable]
    public class IdentityConfigurationSectionCorruptedException : Exception
    {
        public IdentityConfigurationSectionCorruptedException() : base(Resources.ERR_IDENTITY_PROVIDERS_CONFIGURATION_CORRUPTED) { }
        public IdentityConfigurationSectionCorruptedException(Exception inner) : base(Resources.ERR_IDENTITY_PROVIDERS_CONFIGURATION_CORRUPTED, inner) { }
        protected IdentityConfigurationSectionCorruptedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
