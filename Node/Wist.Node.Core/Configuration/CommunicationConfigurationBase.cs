using Wist.Core.Architecture;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    public abstract class CommunicationConfigurationBase : ConfigurationSectionBase
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
