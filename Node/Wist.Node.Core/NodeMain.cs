﻿using CommonServiceLocator;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using System;
using System.Net;
using Wist.Core.Configuration;
using Wist.BlockLattice.Core.Interfaces;
using System.Threading;
using Wist.Node.Core.Configuration;

namespace Wist.Node.Core
{
    /// <summary>
    /// Main class with business logic of Node.
    /// 
    /// Process of start-up:
    ///  1. Initialize - it creates, initializes and launches listeners of other nodes and wallet accounts
    ///  2. EnterGroup - before Node can start to function it must to connect to some consensus group of Nodes for consensus decisions accepting
    ///  3. Start - after Node entered to any consensus group it starts to work
    /// </summary>
    internal class NodeMain
    {
        private static NodeMain _instance;
        private static readonly object _sync = new object();
        private readonly IConfigurationService _configurationService;
        private readonly ICommunicationHub _communicationHubNodes;
        private readonly ICommunicationHub _communicationHubAccounts;
        private readonly IBlocksProcessor _blocksProcessor;
        private readonly CancellationTokenSource _cancellationTokenSource;

        internal NodeMain(IConfigurationService configurationService, ICommunicationHub communicationHubNodes, ICommunicationHub communicationHubAccounts, IBlocksProcessor blocksProcessor)
        {
            _configurationService = configurationService;
            _communicationHubNodes = communicationHubNodes;
            _communicationHubAccounts = communicationHubAccounts;
            _blocksProcessor = blocksProcessor;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        internal void Initialize()
        {
            // TODO: add accounts listener
            NodesCommunicationConfiguration nodesCommunicationConfiguration = (NodesCommunicationConfiguration)_configurationService["nodesCommunication"];
            _communicationHubNodes.Init(
                new SocketListenerSettings(
                    nodesCommunicationConfiguration.MaxConnections,
                    nodesCommunicationConfiguration.MaxPendingConnections,
                    nodesCommunicationConfiguration.MaxSimultaneousAcceptOps,
                    nodesCommunicationConfiguration.ReceiveBufferSize, 2, 
                    new IPEndPoint(IPAddress.Loopback, nodesCommunicationConfiguration.ListeningPort), false));

            _communicationHubNodes.StartListen();
        }

        internal void UpdateKnownNodes()
        {

        }

        internal void EnterNodesGroup()
        {

        }

        internal void Start()
        {
            _blocksProcessor.Initialize(_cancellationTokenSource.Token);
        }
    }
}
