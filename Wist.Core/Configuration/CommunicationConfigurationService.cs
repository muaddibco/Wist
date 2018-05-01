using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Configuration
{
    [RegisterDefaultImplementation(typeof(ICommunicationConfigurationService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class CommunicationConfigurationService : ICommunicationConfigurationService
    {
        private readonly IAppConfig _appConfig;
        private string _sectionName = null;

        public CommunicationConfigurationService(IAppConfig appConfig)
        {
            _appConfig = appConfig;
        }

        public ushort MaxConnections => (ushort)_appConfig.GetLong($"{(string.IsNullOrEmpty(_sectionName) ? nameof(CommunicationConfigurationService) : _sectionName)}:{nameof(MaxConnections)}");

        public ushort MaxPendingConnections => (ushort)_appConfig.GetLong($"{(string.IsNullOrEmpty(_sectionName) ? nameof(CommunicationConfigurationService) : _sectionName)}:{nameof(MaxPendingConnections)}");

        public ushort MaxSimultaneousAcceptOps => (ushort)_appConfig.GetLong($"{(string.IsNullOrEmpty(_sectionName) ? nameof(CommunicationConfigurationService) : _sectionName)}:{nameof(MaxSimultaneousAcceptOps)}");

        public ushort ReceiveBufferSize => (ushort)_appConfig.GetLong($"{(string.IsNullOrEmpty(_sectionName) ? nameof(CommunicationConfigurationService) : _sectionName)}:{nameof(ReceiveBufferSize)}");

        public ushort ListeningPort => (ushort)_appConfig.GetLong($"{(string.IsNullOrEmpty(_sectionName) ? nameof(CommunicationConfigurationService) : _sectionName)}:{nameof(ListeningPort)}");

        public string SectionName { set { _sectionName = value; } }
    }
}
