using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Core.Logging
{
    [ConfigurationSectionSupport]
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class LogConfiguration : IConfigurationSection
    {
        public const string SECTION_NAME = "logging";
        public string SectionName => SECTION_NAME;

        public bool MeasureTime { get; set; }
        public string LogConfigurationFile { get; set; }
    }
}
