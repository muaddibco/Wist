using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Properties;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class FailedToFindBlockForIdentityException : Exception
    {
        public FailedToFindBlockForIdentityException() { }
        public FailedToFindBlockForIdentityException(IKey key) : base(string.Format(Resources.ERR_FAILED_TO_FIND_BLOCK_BY_IDENTITY_KEY, key)) { }
        public FailedToFindBlockForIdentityException(IKey key, Exception inner) : base(string.Format(Resources.ERR_FAILED_TO_FIND_BLOCK_BY_IDENTITY_KEY, key), inner) { }
        protected FailedToFindBlockForIdentityException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
