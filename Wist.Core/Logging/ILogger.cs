using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Logging
{
    [ServiceContract]
    public interface ILogger
    {
        LogSettings Settings { get; }

        void ExceptionError(Enum category, Exception ex, string msg, params object[] messageArgs);
        void Error(Enum category, string msg, params object[] messageArgs);

        void Warning(Enum category, string msg, params object[] messageArgs);
        void Info(Enum category, string msg, params object[] messageArgs);
        void Debug(Enum category, string msg, params object[] messageArgs);
    }
}
