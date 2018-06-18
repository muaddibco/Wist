using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class GeneralUdpCommunicationConfiguration : UdpCommunicationConfigurationBase
    {
        public const string SECTION_NAME = "generalUdpCommunication";
        public override string SectionName => SECTION_NAME;
    }
}
