using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Logging
{
    [ServiceContract]
    public interface ILogger
    {
        void Initialize(string scopeName);

        void ExceptionError(Exception ex, string msg, params object[] messageArgs);
        void Error(string msg, params object[] messageArgs);
        void Warning(string msg, params object[] messageArgs);
        void Info(string msg, params object[] messageArgs);
        void Debug(string msg, params object[] messageArgs);
    }
}
