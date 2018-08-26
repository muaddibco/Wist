using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Synchronization
{

    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationConfiguration : ConfigurationSectionBase, ISynchronizationConfiguration
    {
        public const string SECTION_NAME = "sync";
        public SynchronizationConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }

        public string CommunicationServiceName { get; set; }
    }
}
