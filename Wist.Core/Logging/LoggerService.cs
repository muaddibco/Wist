using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Logging
{
    [RegisterDefaultImplementation(typeof(ILoggerService), Lifetime = LifetimeManagement.Singleton)]
    public class LoggerService : ILoggerService
    {
        private readonly Dictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();

        public ILogger GetLogger(string scopeName)
        {
            if(!_loggers.ContainsKey(scopeName))
            {
                _loggers.Add(scopeName, ServiceLocator.Current.GetInstance<ILogger>());
            }

            return _loggers[scopeName];
        }
    }
}
