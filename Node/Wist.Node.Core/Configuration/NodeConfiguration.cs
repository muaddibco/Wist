using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class NodeConfiguration : ConfigurationSectionBase, INodeConfiguration
    {
        public const string SECTION_NAME = "node";

        public NodeConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }

        [Optional]
        public string[] Modules { get; set; }

        [Optional]
        public string[] CommunicationServices { get; set; }
    }
}
