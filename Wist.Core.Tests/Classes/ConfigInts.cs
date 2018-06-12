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
    public class ConfigInts : IConfigurationSection
    {
        public string SectionName => nameof(ConfigInts);

        public int[] Ints { get; set; }
    }
}
