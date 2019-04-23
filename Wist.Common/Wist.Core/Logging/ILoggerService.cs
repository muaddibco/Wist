using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Logging
{
    [ServiceContract]
    public interface ILoggerService
    {
        ILogger GetLogger(string scopeName);
    }
}
