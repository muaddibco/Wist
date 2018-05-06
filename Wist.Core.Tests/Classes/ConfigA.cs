using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Core.Tests.Classes
{
    [ConfigurationSectionSupport]
    public class ConfigA : IConfigurationSection
    {
        public string SectionName => nameof(ConfigA);
        public ushort MaxValue { get; set; }
    }
}
