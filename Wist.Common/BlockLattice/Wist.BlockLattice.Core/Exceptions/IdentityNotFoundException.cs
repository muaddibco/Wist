using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class IdentityNotFoundException : Exception
    {
        public IdentityNotFoundException() { }
        public IdentityNotFoundException(string hashValue) : base(string.Format(Resources.ERR_IDENTITY_NOT_FOUND, hashValue)) { }
        public IdentityNotFoundException(string hashValue, Exception inner) : base(string.Format(Resources.ERR_IDENTITY_NOT_FOUND, hashValue), inner) { }
        protected IdentityNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
