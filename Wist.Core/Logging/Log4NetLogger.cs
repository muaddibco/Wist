using CommonServiceLocator;
using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Exceptions;

namespace Wist.Core.Logging
{
    [RegisterDefaultImplementation(typeof(ILogger), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class Log4NetLogger : ILogger
    {
        private ILog _log;

        private static bool _isInitialized;
        private static readonly object _sync = new object();
        private readonly IConfigurationService _configurationService;
        private static string _logRepositoryName;

        public Log4NetLogger(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public void Initialize(string scopeName)
        {
            if(!_isInitialized)
            {
                lock(_sync)
                {
                    if(!_isInitialized)
                    {
                        ConfigureLog4Net(_configurationService.Get<ILogConfiguration>()?.LogConfigurationFile);

                        _isInitialized = true;
                    }
                }
            }

            _log = LogManager.GetLogger(_logRepositoryName, scopeName);
        }

        public void Debug(string msg, params object[] messageArgs)
        {
            string formattedMessage = FormatMessage(msg, messageArgs);

            _log.Debug(formattedMessage);
        }

        public void Info(string msg, params object[] messageArgs)
        {
            string formattedMessage = FormatMessage(msg, messageArgs);
            _log.Info(formattedMessage);
        }

        public void Warning(string msg, params object[] messageArgs)
        {
            string formattedMessage = FormatMessage(msg, messageArgs);
            _log.Warn(formattedMessage);
        }

        public void Error(string msg, params object[] messageArgs)
        {
            string formattedMessage = FormatMessage(msg, messageArgs);

            _log.Error(formattedMessage);
        }

        public void Error(string msg, Exception ex, params object[] messageArgs)
        {
            if (ex == null)
            {
                Error(msg, messageArgs);
                return;
            }

            string formattedMessage = FormatMessage(msg, messageArgs);
            //string messageWithException = $"{formattedMessage} - Exception: {ex.GetType()}, {ex.Message} - {ex}";


            _log.Error(formattedMessage, ex);
        }

        private void ConfigureLog4Net(string logConfigFilePath)
        {
            if (!string.IsNullOrEmpty(logConfigFilePath))
            {
                FileInfo executinAssemblyInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                if (executinAssemblyInfo.DirectoryName != null)
                {
                    string path = Path.Combine(executinAssemblyInfo.DirectoryName, logConfigFilePath);
                    if (File.Exists(path) == false)
                    {
                        throw new FailedToFindLogConfigFileException("log4net", path);
                    }
                    
                    //TODO: ascertain XmlConfigurator works as expected
                    XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetExecutingAssembly()), new FileInfo(path));

                    return;
                }
            }

            ILoggerRepository loggerRepository = LogManager.GetRepository(Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(loggerRepository);

            _logRepositoryName = loggerRepository.Name;
        }

        private static string FormatMessage(string msg, object[] messageArgs)
        {
            if (messageArgs == null || messageArgs.Length == 0)
            {
                return msg ?? string.Empty;
            }

            return string.Format(msg ?? string.Empty, messageArgs);
        }
    }
}
