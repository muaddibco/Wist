using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Core.Tests.Classes
{
    [ConfigurationSectionSupport]
    public class ConfigB : IConfigurationSection
    {
        public string SectionName => nameof(ConfigB);

        public ushort MaxValue { get; set; }
    }
}
