using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Network.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class GeneralTcpCommunicationConfiguration : CommunicationConfigurationBase
    {
        public const string SECTION_NAME = "generalTcpCommunication";

        public GeneralTcpCommunicationConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }
    }
}
