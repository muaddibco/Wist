using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    [ConfigurationSectionSupport]
    public abstract class UdpCommunicationConfigurationBase : IConfigurationSection
    {
        public ushort ReceiveBufferSize { get; set; }

        public ushort ListeningPort { get; set; }

        public abstract string SectionName { get; }
    }
}
