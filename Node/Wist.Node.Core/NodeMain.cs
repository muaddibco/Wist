using CommonServiceLocator;
using CommunicationLibrary.Interfaces;
using CommunicationLibrary.Sockets;
using System;
using System.Net;
using Wist.Core.Configuration;

namespace Wist.Node.Core
{
    public class NodeMain
    {
        private static NodeMain _instance;
        private static readonly object _sync = new object();
        private readonly IConfigurationService _configurationService;
        private readonly ICommunicationHub _communicationHubNodes;
        private readonly ICommunicationHub _communicationHubAccounts;

        private NodeMain(IConfigurationService configurationService, ICommunicationHub communicationHubNodes, ICommunicationHub communicationHubAccounts)
        {
            _configurationService = configurationService;
            _communicationHubNodes = communicationHubNodes;
            _communicationHubAccounts = communicationHubAccounts;
        }

        public NodeMain Instance
        {
            get
            {
                if(_instance == null)
                {
                    lock(_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = ServiceLocator.Current.GetInstance<NodeMain>();
                        }
                    }
                }

                return _instance;
            }
        }

        public void Initialize()
        {
            _communicationHubNodes.Init(
                new SocketListenerSettings(
                    _configurationService.NodesCommunicationConfiguration.MaxConnections, 
                    _configurationService.NodesCommunicationConfiguration.MaxPendingConnections, 
                    _configurationService.NodesCommunicationConfiguration.MaxSimultaneousAcceptOps, 
                    _configurationService.NodesCommunicationConfiguration.ReceiveBufferSize, 2, 
                    new IPEndPoint(IPAddress.Loopback, _configurationService.NodesCommunicationConfiguration.ListeningPort), false));

            _communicationHubNodes.StartListen();
        }

        public void EnterNodesGroup()
        {

        }
    }
}
