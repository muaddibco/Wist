using CommonServiceLocator;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using System;
using System.Net;
using Wist.Core.Configuration;
using Wist.BlockLattice.Core.Interfaces;
using System.Threading;
using Wist.Node.Core.Configuration;
using Wist.Node.Core.Interfaces;

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
        private static readonly object _sync = new object();
        private readonly IConfigurationService _configurationService;
        private readonly ISynchronizationService _synchronizationService;
        private readonly ICommunicationHub _communicationHubNodes;
        private readonly ICommunicationHub _communicationHubAccounts;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private IBlocksProcessor _consensusBlocksProcessor;
        private IBlocksProcessor _synchronizationBlocksProcessor;

        //private read-only IBlocksProcessor _blocksProcessor

        internal NodeMain(IConfigurationService configurationService, ICommunicationHubFactory communicationHubFactory, IBlocksProcessorFactory blocksProcessorFactory, ISynchronizationService synchronizationService)
        {
            _configurationService = configurationService;
            _synchronizationService = synchronizationService;
            _communicationHubNodes = communicationHubFactory.Create();
            _communicationHubAccounts = communicationHubFactory.Create();
            _consensusBlocksProcessor = blocksProcessorFactory.Create(ConsensusBlocksProcessor.BLOCKS_PROCESSOR_NAME);
            _synchronizationBlocksProcessor = blocksProcessorFactory.Create(SynchronizationBlocksProcessor.BLOCKS_PROCESSOR_NAME);

            _cancellationTokenSource = new CancellationTokenSource();
        }

        internal void Initialize()
        {
            //TODO: add accounts listener
            NodesCommunicationConfiguration nodesCommunicationConfiguration = (NodesCommunicationConfiguration)_configurationService[NodesCommunicationConfiguration.SECTION_NAME];
            AccountsCommunicationConfiguration accountsCommunicationConfiguration = (AccountsCommunicationConfiguration)_configurationService[AccountsCommunicationConfiguration.SECTION_NAME];

            _communicationHubNodes.Init(
                new SocketListenerSettings(
                    nodesCommunicationConfiguration.MaxConnections,
                    nodesCommunicationConfiguration.MaxPendingConnections,
                    nodesCommunicationConfiguration.MaxSimultaneousAcceptOps,
                    nodesCommunicationConfiguration.ReceiveBufferSize, 2, 
                    new IPEndPoint(IPAddress.Loopback, nodesCommunicationConfiguration.ListeningPort), false), _consensusBlocksProcessor);

            //TODO: need to understand what blocks processor need to provide here
            _communicationHubAccounts.Init(new SocketListenerSettings(
                accountsCommunicationConfiguration.MaxConnections,
                accountsCommunicationConfiguration.MaxPendingConnections,
                accountsCommunicationConfiguration.MaxSimultaneousAcceptOps,
                accountsCommunicationConfiguration.ReceiveBufferSize, 2,
                new IPEndPoint(IPAddress.Loopback, accountsCommunicationConfiguration.ListeningPort), false), null);

            _communicationHubNodes.StartListen();
            _communicationHubAccounts.StartListen();


            _synchronizationService.Initialize(_synchronizationBlocksProcessor, _cancellationTokenSource.Token);
        }

        internal void UpdateKnownNodes()
        {

        }

        internal void EnterNodesGroup()
        {

        }

        internal void Start()
        {
            _consensusBlocksProcessor.Initialize(_cancellationTokenSource.Token);
            _synchronizationService.Start();
        }
    }
}
