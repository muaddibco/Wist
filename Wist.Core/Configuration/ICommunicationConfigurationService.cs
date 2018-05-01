using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;

namespace Wist.Core.Configuration
{
    [ServiceContract]
    public interface ICommunicationConfigurationService
    {
        string SectionName { set; }
        ushort MaxConnections { get; }
        ushort MaxPendingConnections { get; }
        ushort MaxSimultaneousAcceptOps { get; }
        ushort ReceiveBufferSize { get; }
        ushort ListeningPort { get; }
    }
}
