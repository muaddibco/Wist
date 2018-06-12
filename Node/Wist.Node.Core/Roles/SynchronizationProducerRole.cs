using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Roles
{
    [RegisterExtension(typeof(IRole), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducerRole : IRole
    {
        private readonly ICommunicationServicesFactory _communicationServicesFactory;
        private readonly ICommunicationService _syncGroupCommunicationService;
        private readonly IBlocksProcessor _blocksProcessor;

        public SynchronizationProducerRole(ICommunicationServicesFactory communicationServicesFactory)
        {
            _communicationServicesFactory = communicationServicesFactory;
            _syncGroupCommunicationService = communicationServicesFactory.Create("TcpCommunicationService");
        }
        public string Name => nameof(SynchronizationProducerRole);

        public void Initialize()
        {
            SocketListenerSettings settings = new SocketListenerSettings(21, 1024, new IPEndPoint(IPAddress.Any, 5021));
            _syncGroupCommunicationService.Init(settings, _blocksProcessor);
        }

        public Task Play()
        {
            return Task.Run(() => { _syncGroupCommunicationService.Start(); });
        }
    }
}
