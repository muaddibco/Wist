using Wist.Communication.Properties;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Wist.Communication.Exceptions
{
    [Serializable]
    public class NotUniqueResponseFactoriesException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public NotUniqueResponseFactoriesException()
        {
        }

        public NotUniqueResponseFactoriesException(string message) : base(string.Format(Resources.ERR_NOT_UNIQUE_RESPONSE_FACTORIES, message))
        {
        }

        public NotUniqueResponseFactoriesException(string message, Exception inner) : base(string.Format(Resources.ERR_NOT_UNIQUE_RESPONSE_FACTORIES, message), inner)
        {
        }

        protected NotUniqueResponseFactoriesException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
