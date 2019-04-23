using System.Configuration;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.Configuration
{
    [RegisterDefaultImplementation(typeof(IAppConfig), Lifetime = LifetimeManagement.Singleton)]
    public class AppConfig : IAppConfig
    {
        public bool GetBool(string key, bool required = true)
        {
            string value = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return false;
            }

            bool bValue;

            if (!bool.TryParse(ConfigurationManager.AppSettings.Get(key), out bValue))
                throw new ConfigurationParameterInvalidValueException(key, value, "true or false");

            return bValue;
        }

        public long GetLong(string key, bool required = true)
        {
            string value = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return 0;
            }
            long lValue;

            if (!long.TryParse(ConfigurationManager.AppSettings.Get(key), out lValue))
                throw new ConfigurationParameterInvalidValueException(key, value, "numeric value");

            return lValue;
        }

        public string GetString(string key, bool required = true)
        {
            string value = ConfigurationManager.AppSettings.Get(key);

            if (string.IsNullOrEmpty(value))
            {
                if (required)
                    throw new RequiredConfigurationParameterNotSpecifiedException(key);

                return null;
            }

            return ConfigurationManager.AppSettings.Get(key);
        }
    }
}
