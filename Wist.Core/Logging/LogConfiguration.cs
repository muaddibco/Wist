using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Core.Logging
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class LogConfiguration : ConfigurationSectionBase, ILogConfiguration
    {
        public const string SECTION_NAME = "logging";

        public LogConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }

        [Optional]
        public bool MeasureTime { get; set; }

        [Optional]
        public string LogConfigurationFile { get; set; }
    }
}
