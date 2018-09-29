using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Properties;

namespace Wist.BlockLattice.Core.Exceptions
{

    [Serializable]
    public class PreviousHashNotProvidedException : Exception
    {
        public PreviousHashNotProvidedException() : base(Resources.ERR_PREV_HASH_NOT_PROVIDED) { }
        public PreviousHashNotProvidedException(Exception inner) : base(Resources.ERR_PREV_HASH_NOT_PROVIDED, inner) { }
        protected PreviousHashNotProvidedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
