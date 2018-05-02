using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;

namespace Wist.Core.Configuration
{
    [ConfigurationSectionSupport]
    public class CommunicationConfigurationService : IConfigurationSectionSupport
    {
        private readonly string _sectionName;

        public CommunicationConfigurationService(string sectionName)
        {
            _sectionName = sectionName;
        }

        public ushort MaxConnections { get; set; }

        public ushort MaxPendingConnections { get; set; }

        public ushort MaxSimultaneousAcceptOps { get; set; }

        public ushort ReceiveBufferSize { get; set; }

        public ushort ListeningPort { get; set; }

        public string SectionName => _sectionName;
    }
}
