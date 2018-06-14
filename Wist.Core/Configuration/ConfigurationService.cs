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

        public T Get<T>(string sectionName) where T : class, IConfigurationSection
        {
            return this[sectionName] as T;
        }
    }
}
