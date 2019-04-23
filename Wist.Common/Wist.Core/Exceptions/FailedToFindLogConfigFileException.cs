using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Wist.Core.Properties;

namespace Wist.Core.Exceptions
{
    [Serializable]
    public class FailedToFindLogConfigFileException : Exception
    {
        public FailedToFindLogConfigFileException() { }
        public FailedToFindLogConfigFileException(string logComponent, string path) : base(string.Format(Resources.ERR_FAILED_TO_FIND_LOG_CONFIG_FILE, logComponent, path)) { }
        public FailedToFindLogConfigFileException(string logComponent, string path, Exception inner) : base(string.Format(Resources.ERR_FAILED_TO_FIND_LOG_CONFIG_FILE, logComponent, path), inner) { }
        protected FailedToFindLogConfigFileException(
          SerializationInfo info,
          StreamingContext context) : base(info, context)
        { }
    }
}
