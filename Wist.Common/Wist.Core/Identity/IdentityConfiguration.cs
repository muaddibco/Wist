using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Core.Identity
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class IdentityConfiguration : ConfigurationSectionBase, IIdentityConfiguration
    {
        public const string SECTION_NAME = "identity";

        public IdentityConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }

        public string Provider { get; set; }
    }
}
