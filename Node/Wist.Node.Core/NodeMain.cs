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
using Wist.Core;

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
        private readonly ICommunicationServicesRepository _communicationServicesFactory;
        private readonly ICommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IConfigurationService _configurationService;
        private readonly IModulesRepository _modulesRepository;
        private readonly IRolesRegistry _rolesRegistry;
        private readonly IInitializer[] _initializers;
        private readonly CancellationTokenSource _cancellationTokenSource;

        //private read-only IBlocksProcessor _blocksProcessor

        public NodeMain(ICommunicationServicesRepository communicationServicesFactory, ICommunicationServicesRegistry communicationServicesRegistry, IConfigurationService configurationService, IModulesRepository modulesRepository, IRolesRegistry rolesRegistry, IBlocksHandlersFactory blocksProcessorFactory, ILoggerService loggerService, IInitializer[] initializers)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _communicationServicesFactory = communicationServicesFactory;
            _communicationServicesRegistry = communicationServicesRegistry;
            _configurationService = configurationService;
            _modulesRepository = modulesRepository;
            _rolesRegistry = rolesRegistry;
            _initializers = initializers;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize()
        {
            InitializeGlobals();

            InitializeCommunicationLayer();

            ObtainConfiguredModules();

            InitializeModules();
        }

        private void InitializeGlobals()
        {
            foreach (IInitializer item in _initializers)
            {
                try
                {
                    item.Initialize();
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to initialize {item.GetType().FullName}", ex);
                }
            }
        }

        private void InitializeCommunicationLayer()
        {
            ICommunicationService communicationServiceTcp = _communicationServicesFactory.GetInstance("GenericTcp");
            ICommunicationService communicationServiceUdp = _communicationServicesFactory.GetInstance("GenericUdp");

            GeneralTcpCommunicationConfiguration generalTcpCommunicationConfiguration = _configurationService.Get<GeneralTcpCommunicationConfiguration>();
            GeneralUdpCommunicationConfiguration generalUdpCommunicationConfiguration = _configurationService.Get<GeneralUdpCommunicationConfiguration>();

            communicationServiceTcp.Init(new SocketListenerSettings(generalTcpCommunicationConfiguration.MaxConnections, generalTcpCommunicationConfiguration.ReceiveBufferSize, new IPEndPoint(IPAddress.Any, generalTcpCommunicationConfiguration.ListeningPort)));
            communicationServiceUdp.Init(new SocketListenerSettings(1, generalUdpCommunicationConfiguration.ReceiveBufferSize, new IPEndPoint(IPAddress.Any, generalUdpCommunicationConfiguration.ListeningPort)));

            _communicationServicesRegistry.RegisterInstance(communicationServiceTcp, "GenericTcp");
            _communicationServicesRegistry.RegisterInstance(communicationServiceUdp, "GenericUdp");
        }

        private void InitializeModules()
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

        private void ObtainConfiguredModules()
        {
            NodeConfiguration nodeConfiguration = (NodeConfiguration)_configurationService[NodeConfiguration.SECTION_NAME];

            string[] moduleNames = nodeConfiguration.Modules;
            if (moduleNames != null)
            {
                foreach (string moduleName in moduleNames)
                {
                    try
                    {
                        IModule module = _modulesRepository.GetInstance(moduleName);
                        _modulesRepository.RegisterInstance(module);
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Failed to register Module with name '{moduleName}'.", ex);
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
            _communicationServicesRegistry.GetInstance("GenericTcp").Start();
            _communicationServicesRegistry.GetInstance("GenericUdp").Start();

            IEnumerable<IRole> roles = _rolesRegistry.GetBulkInstances();

            foreach (IRole role in roles)
            {
                role.Start();
            }
        }
    }
}
