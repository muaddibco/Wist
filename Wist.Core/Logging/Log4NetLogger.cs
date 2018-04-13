using log4net;
using log4net.Config;
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
    [RegisterDefaultImplementation(typeof(ILogger), Lifetime = LifetimeManagement.Singleton)]
    public class Log4NetLogger : ILogger
    {
        private readonly Dictionary<Enum, ILog> _loggers = new Dictionary<Enum, ILog>();

        public LogSettings Settings { get; }

        public Log4NetLogger(IAppConfig appConfig)
        {
            Settings = new LogSettings
            {
                Log4NetConfigurationFile = appConfig.GetString("Log4NetConfigurationFile", false),
                MeasureTime = appConfig.GetBool("LogMeasureTime", false)
            };

            ConfigureLog4Net();
        }

        private void ConfigureLog4Net()
        {
            if (!string.IsNullOrEmpty(Settings.Log4NetConfigurationFile))
            {
                FileInfo executinAssemblyInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
                if (executinAssemblyInfo.DirectoryName != null)
                {
                    string path = Path.Combine(executinAssemblyInfo.DirectoryName, Settings.Log4NetConfigurationFile);
                    if (File.Exists(path) == false)
                    {
                        throw new FailedToFindLogConfigFileException("log4net", path);
                    }
                    
                    // TODO: ascertain XmlConfigurator works as expected
                    XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetExecutingAssembly()), new FileInfo(path));

                    return;
                }
            }

            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetExecutingAssembly()));
        }

        private ILog GetLog4NetLogger(Enum loggerCategory)
        {
            if (loggerCategory == null)
            {
                loggerCategory = NullEnum.Null;
            }

            if (!_loggers.ContainsKey(loggerCategory))
            {
                _loggers[loggerCategory] = LogManager.GetLogger(Assembly.GetExecutingAssembly(), loggerCategory.ToString());
            }

            return _loggers[loggerCategory];
        }

        private static string FormatMessage(string msg, object[] messageArgs)
        {
            if (messageArgs == null || messageArgs.Length == 0)
            {
                return msg ?? string.Empty;
            }

            return string.Format(msg ?? string.Empty, messageArgs);
        }

        public void Debug(Enum category, string msg, params object[] messageArgs)
        {
            string formattedMessage = FormatMessage(msg, messageArgs);

            GetLog4NetLogger(category).Debug(formattedMessage);
        }

        public void Info(Enum category, string msg, params object[] messageArgs)
        {
            string formattedMessage = FormatMessage(msg, messageArgs);
            GetLog4NetLogger(category).Info(formattedMessage);
        }

        public void Warning(Enum category, string msg, params object[] messageArgs)
        {
            string formattedMessage = FormatMessage(msg, messageArgs);
            GetLog4NetLogger(category).Warn(formattedMessage);
        }

        public void Error(Enum category, string msg, params object[] messageArgs)
        {
            string formattedMessage = FormatMessage(msg, messageArgs);

            GetLog4NetLogger(category).Error(formattedMessage);
        }

        public void ExceptionError(Enum category, Exception ex, string msg, params object[] messageArgs)
        {
            if (ex == null)
            {
                Error(category, msg, messageArgs);
                return;
            }

            string formattedMessage = FormatMessage(msg, messageArgs);
            string messageWithException = $"{formattedMessage} - Exception: {ex.GetType()}, {ex.Message} - {ex}";


            GetLog4NetLogger(category).Error(messageWithException);
        }

        private enum NullEnum
        {
            Null
        };
    }
}
