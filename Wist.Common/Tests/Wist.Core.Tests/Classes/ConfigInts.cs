using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Core.Tests.Classes
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigInts : ConfigurationSectionBase
    {
        public ConfigInts(IApplicationContext applicationContext) : base(applicationContext, nameof(ConfigInts))
        {
        }

        public int[] Ints { get; set; }
    }
}
