using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Interaction
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class InteractionConfiguration : ConfigurationSectionBase, IInteractionConfiguration
    {
        public const string SECTION_NAME = "interaction";

        public InteractionConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }

        public int Port { get ; set; }
    }
}
