using System;
using Wist.Node.Core.Properties;

namespace Wist.Node.Core.Exceptions
{

    [Serializable]
    public class MemPoolOfElementsNotSupportedException : Exception
    {
        public MemPoolOfElementsNotSupportedException() { }
        public MemPoolOfElementsNotSupportedException(Type type) : base(string.Format(Resources.ERR_MEMPOOL_OF_ELEMENTS_NOT_SUPPORTED, type.FullName)) { }
        public MemPoolOfElementsNotSupportedException(Type type, Exception inner) : base(string.Format(Resources.ERR_MEMPOOL_OF_ELEMENTS_NOT_SUPPORTED, type.FullName), inner) { }
        protected MemPoolOfElementsNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
