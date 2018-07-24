using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Core.Tests.Classes
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigB : ConfigurationSectionBase
    {
        public ConfigB(IApplicationContext applicationContext) : base(applicationContext, nameof(ConfigB))
        {
        }

        public ushort MaxValue { get; set; }
    }
}
