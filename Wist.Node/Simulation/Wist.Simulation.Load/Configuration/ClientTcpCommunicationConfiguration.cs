﻿using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.Configuration;
using Wist.Network.Configuration;

namespace Wist.Simulation.Load.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ClientTcpCommunicationConfiguration : CommunicationConfigurationBase
    {
        public const string SECTION_NAME = "clientTcpCommunication";

        public ClientTcpCommunicationConfiguration(IApplicationContext applicationContext) : base(applicationContext, SECTION_NAME)
        {
        }
    }
}
