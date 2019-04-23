using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Core.Tests.Classes
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigA : ConfigurationSectionBase
    {
        public ConfigA(IApplicationContext applicationContext) : base(applicationContext, nameof(ConfigA))
        {
        }
        public ushort MaxValue { get; set; }
    }
}
