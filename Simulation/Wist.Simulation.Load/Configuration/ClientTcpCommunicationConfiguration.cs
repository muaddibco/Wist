using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Node.Core.Configuration;

namespace Wist.Simulation.Load.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ClientTcpCommunicationConfiguration : TcpCommunicationConfigurationBase
    {
        public const string SECTION_NAME = "clientTcpCommunication";
        public override string SectionName => SECTION_NAME;
    }
}
