using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Core.Tests.Classes
{
    [ConfigurationSectionSupport]
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigRoles : IConfigurationSection
    {
        public string SectionName => nameof(ConfigRoles);

        public string[] Roles { get; set; }
    }
}
