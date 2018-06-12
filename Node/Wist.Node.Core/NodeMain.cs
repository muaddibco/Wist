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
using System.Collections;
using System.Collections.Generic;

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
        private readonly IRolesRepository _rolesRepository;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private IBlocksProcessor _consensusBlocksProcessor;
        private IBlocksProcessor _synchronizationBlocksProcessor;

        //private read-only IBlocksProcessor _blocksProcessor

        internal NodeMain(IConfigurationService configurationService, IRolesRepository rolesRepository, IBlocksProcessorFactory blocksProcessorFactory, ISynchronizationService synchronizationService)
        {
            _configurationService = configurationService;
            _synchronizationService = synchronizationService;
            _rolesRepository = rolesRepository;
            _consensusBlocksProcessor = blocksProcessorFactory.Create(ConsensusBlocksProcessor.BLOCKS_PROCESSOR_NAME);
            _synchronizationBlocksProcessor = blocksProcessorFactory.Create(SynchronizationBlocksProcessor.BLOCKS_PROCESSOR_NAME);

            _cancellationTokenSource = new CancellationTokenSource();
        }

        internal void Initialize()
        {
            IEnumerable<IRole> roles = _rolesRepository.GetBulkInstances();

            foreach (IRole role in roles)
            {
                role.Initialize();
            }


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

            IEnumerable<IRole> roles = _rolesRepository.GetBulkInstances();

            foreach (IRole role in roles)
            {
                role.Play();
            }
        }
    }
}
