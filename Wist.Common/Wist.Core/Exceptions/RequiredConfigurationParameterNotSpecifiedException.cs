using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{
    [Serializable]
    public class RequiredConfigurationParameterNotSpecifiedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RequiredConfigurationParameterNotSpecifiedException()
        {
        }

        public RequiredConfigurationParameterNotSpecifiedException(string parameterName) : base(string.Format(Resources.ERR_REQUIRED_CONIGURATION_PARAMETER_NOT_SPECIFIED, parameterName))
        {
        }

        public RequiredConfigurationParameterNotSpecifiedException(string parameterName, Exception inner) : base(string.Format(Resources.ERR_REQUIRED_CONIGURATION_PARAMETER_NOT_SPECIFIED, parameterName), inner)
        {
        }

        protected RequiredConfigurationParameterNotSpecifiedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
