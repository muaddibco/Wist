using Wist.Core.Architecture;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Network.Configuration
{
    public class CommunicationConfigurationBase : ConfigurationSectionBase
    {
        public CommunicationConfigurationBase(IApplicationContext applicationContext, string sectionName) : base(applicationContext, sectionName)
        {
        }

        public string CommunicationServiceName { get; set; }

        public ushort MaxConnections { get; set; }

        public ushort ReceiveBufferSize { get; set; }

        public ushort ListeningPort { get; set; }
    }
}
