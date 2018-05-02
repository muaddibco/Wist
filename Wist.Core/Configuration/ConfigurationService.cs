using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Configuration
{
    [RegisterDefaultImplementation(typeof(IConfigurationService), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigurationService : IConfigurationService
    {
        public ConfigurationService()
        {
            NodesCommunication = new CommunicationConfigurationService(nameof(NodesCommunication));
            AccountsCommunication = new CommunicationConfigurationService(nameof(AccountsCommunication));
        }

        public CommunicationConfigurationService NodesCommunication { get; }
        public CommunicationConfigurationService AccountsCommunication { get; }
    }
}
