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
        private readonly ICommunicationServicesFactory _communicationServicesFactory;
        private readonly ICommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IConfigurationService _configurationService;
        private readonly IModulesRepository _modulesRepository;
        private readonly CancellationTokenSource _cancellationTokenSource;

        //private read-only IBlocksProcessor _blocksProcessor

        public NodeMain(ICommunicationServicesFactory communicationServicesFactory, ICommunicationServicesRegistry communicationServicesRegistry, IConfigurationService configurationService, IModulesRepository modulesRepository, IBlocksHandlersFactory blocksProcessorFactory, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _communicationServicesFactory = communicationServicesFactory;
            _communicationServicesRegistry = communicationServicesRegistry;
            _configurationService = configurationService;
            _modulesRepository = modulesRepository;

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            ObtainConfiguredRoles();

            InitializeRoles();
        }

        private void InitializeCommunicationLayer()
        {
            ICommunicationService communicationServiceTcp = _communicationServicesFactory.Create("GenericTcp");
            ICommunicationService communicationServiceUdp = _communicationServicesFactory.Create("GenericUdp");

            _communicationServicesRegistry.RegisterInstance(communicationServiceTcp, "GenericTcp");
            _communicationServicesRegistry.RegisterInstance(communicationServiceUdp, "GenericUdp");
        }

        private void InitializeRoles()
        {
            IEnumerable<IModule> modules = _modulesRepository.GetBulkInstances();

            foreach (IModule module in modules)
            {
                try
                {
                    module.Initialize();
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to initialize Module '{module.Name}'", ex);
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
                        IModule role = _modulesRepository.GetInstance(roleName);
                        _modulesRepository.RegisterInstance(role);
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
            IEnumerable<IModule> modules = _modulesRepository.GetBulkInstances();

            foreach (IModule module in modules)
            {
                if (module.IsInitialized)
                {
                    module.Play();
                }
            }
        }
    }
}
