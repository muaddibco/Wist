using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Configuration
{
    [RegisterDefaultImplementation(typeof(IConfigurationService), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationSection[] _configurationSections;

        public ConfigurationService(IConfigurationSection[] configurationSections)
        {
            _configurationSections = configurationSections;
        }

        public IConfigurationSection this[string sectionName] => _configurationSections.FirstOrDefault(s => s.SectionName.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase));

        public T Get<T>() where T : class, IConfigurationSection
        {
            IConfigurationSection configurationSection = _configurationSections.FirstOrDefault(s => s is T);
            return configurationSection as T;
        }
    }
}
