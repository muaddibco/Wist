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
                ILogger logger = ServiceLocator.Current.GetInstance<ILogger>();
                logger.Initialize(scopeName);
                _loggers.Add(scopeName, logger);
            }

            return _loggers[scopeName];
        }
    }
}
