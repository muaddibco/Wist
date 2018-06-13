using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;

namespace Wist.Node.Core.Configuration
{
    [ConfigurationSectionSupport]
    public abstract class CommunicationConfigurationBase : IConfigurationSection
    {
        public CommunicationConfigurationBase()
        {
        }

        public ushort MaxConnections { get; set; }

        public ushort ReceiveBufferSize { get; set; }

        public ushort ListeningPort { get; set; }

        public abstract string SectionName { get; }
    }
}
