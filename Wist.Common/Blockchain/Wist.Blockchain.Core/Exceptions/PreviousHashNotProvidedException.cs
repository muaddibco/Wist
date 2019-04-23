using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.Properties;

namespace Wist.Blockchain.Core.Exceptions
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
