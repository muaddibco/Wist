﻿using CommonServiceLocator;
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
using log4net;
using Wist.Core.Logging;

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
    public class NodeMain
    {
        private static readonly object _sync = new object();
        private readonly ILogger _log;
        private readonly IConfigurationService _configurationService;
        private readonly ISynchronizationService _synchronizationService;
        private readonly IRolesRepository _rolesRepository;
        private readonly CancellationTokenSource _cancellationTokenSource;

        //private read-only IBlocksProcessor _blocksProcessor

        public NodeMain(IConfigurationService configurationService, IRolesRepository rolesRepository, IBlocksProcessorFactory blocksProcessorFactory, ISynchronizationService synchronizationService, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _configurationService = configurationService;
            _synchronizationService = synchronizationService;
            _rolesRepository = rolesRepository;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            ObtainConfiguredRoles();

            InitializeRoles();

            _synchronizationService.Initialize(_cancellationTokenSource.Token);
        }

        private void InitializeRoles()
        {
            IEnumerable<IRole> roles = _rolesRepository.GetBulkInstances();

            foreach (IRole role in roles)
            {
                try
                {
                    role.Initialize();
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to initialize Role '{role.Name}'", ex);
                }
            }
        }

        private void ObtainConfiguredRoles()
        {
            NodeConfiguration nodeConfiguration = (NodeConfiguration)_configurationService[NodeConfiguration.SECTION_NAME];

            string[] roles = nodeConfiguration.Roles;
            if (roles != null)
            {
                foreach (string roleName in roles)
                {
                    try
                    {
                        IRole role = _rolesRepository.GetInstance(roleName);
                        _rolesRepository.RegisterInstance(role);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Failed to register Role with name '{roleName}'.", ex);
                    }
                }
            }
        }

        internal void UpdateKnownNodes()
        {

        }

        internal void EnterNodesGroup()
        {

        }

        internal void Start()
        {
            _synchronizationService.Start();

            IEnumerable<IRole> roles = _rolesRepository.GetBulkInstances();

            foreach (IRole role in roles)
            {
                if (role.IsInitialized)
                {
                    role.Play();
                }
            }
        }
    }
}
