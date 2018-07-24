using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Aspects;

namespace Wist.Core.Configuration
{
    [ConfigurationSectionSupport]
    public abstract class ConfigurationSectionBase : IConfigurationSection
    {
        private readonly IApplicationContext _applicationContext;

        public ConfigurationSectionBase(IApplicationContext applicationContext, string sectionName)
        {
            _applicationContext = applicationContext;
            SectionName = sectionName;
        }

        public string SectionName { get; }

        public IApplicationContext ApplicationContext => _applicationContext;
    }
}
