using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{
    [Serializable]
    public class ConfigurationParameterInvalidValueException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ConfigurationParameterInvalidValueException()
        {
        }

        public ConfigurationParameterInvalidValueException(string parameterName, string value, string expected) : base(string.Format(Resources.ERR_WRONG_CONFIGURATION_VALUE_FORMAT, parameterName, value, expected))
        {
        }

        public ConfigurationParameterInvalidValueException(string parameterName, string value, string expected, Exception inner) : base(string.Format(Resources.ERR_WRONG_CONFIGURATION_VALUE_FORMAT, parameterName, value, expected), inner)
        {
        }

        protected ConfigurationParameterInvalidValueException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
