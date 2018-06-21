using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    [ConfigurationSectionSupport]
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class NodeConfiguration : IConfigurationSection
    {
        public const string SECTION_NAME = "node";

        public string SectionName => SECTION_NAME;

        public string[] Modules { get; set; }
    }
}
