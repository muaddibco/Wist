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
        private readonly ICommunicationConfigurationService _communicationConfigurationServiceNodes;
        private readonly ICommunicationConfigurationService _communicationConfigurationServiceAccounts;

        public ConfigurationService(ICommunicationConfigurationService communicationConfigurationServiceNodes, ICommunicationConfigurationService communicationConfigurationServiceAccounts)
        {
            _communicationConfigurationServiceNodes = communicationConfigurationServiceNodes;
            _communicationConfigurationServiceNodes.SectionName = "nodesCommunication";
            _communicationConfigurationServiceAccounts = communicationConfigurationServiceAccounts;
            _communicationConfigurationServiceAccounts.SectionName = "accountsCommunication";
        }

        public ICommunicationConfigurationService NodesCommunicationConfiguration => _communicationConfigurationServiceNodes;
        public ICommunicationConfigurationService AccountsCommunicationConfiguration => _communicationConfigurationServiceAccounts;
    }
}
