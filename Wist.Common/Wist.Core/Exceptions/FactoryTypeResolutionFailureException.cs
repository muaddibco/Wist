using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{
    [Serializable]
    public class FactoryTypeResolutionFailureException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public FactoryTypeResolutionFailureException()
        {
        }

        public FactoryTypeResolutionFailureException(Type type)
            : base(string.Format(Resources.ERR_FACTORY_TYPE_RESOLUTION_FAILURE, type.FullName))
        {
        }

        public FactoryTypeResolutionFailureException(Type type, Exception inner)
            : base(string.Format(Resources.ERR_FACTORY_TYPE_RESOLUTION_FAILURE, type.FullName), inner)
        {
        }

        protected FactoryTypeResolutionFailureException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
