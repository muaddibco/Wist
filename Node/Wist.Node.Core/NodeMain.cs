using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using System;
using System.Net;
using Wist.Core.Configuration;
using Wist.BlockLattice.Core.Interfaces;
using System.Threading;
using Wist.Node.Core.Configuration;
using System.Collections.Generic;
using Wist.Core.Logging;
using Wist.BlockLattice.Core.Handlers;
using Wist.Node.Core.Modules;

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
        private readonly IServerCommunicationServicesRepository _communicationServicesFactory;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IConfigurationService _configurationService;
        private readonly IModulesRepository _modulesRepository;
        private readonly IPacketsHandler _packetsHandler;
        private readonly CancellationTokenSource _cancellationTokenSource;

        //private read-only IBlocksProcessor _blocksProcessor

        public NodeMain(IServerCommunicationServicesRepository communicationServicesFactory, IServerCommunicationServicesRegistry communicationServicesRegistry, IConfigurationService configurationService, IModulesRepository modulesRepository, IPacketsHandler packetsHandler, IBlocksHandlersRegistry blocksProcessorFactory, ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _communicationServicesFactory = communicationServicesFactory;
            _communicationServicesRegistry = communicationServicesRegistry;
            _configurationService = configurationService;
            _modulesRepository = modulesRepository;
            _packetsHandler = packetsHandler;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Initialize(CancellationToken ct)
        {
            InitializeCommunicationLayer();

            ObtainConfiguredModules();

            InitializeModules(ct);
        }

        private void InitializeCommunicationLayer()
        {
            INodeConfiguration nodeConfiguration = _configurationService.Get<INodeConfiguration>();

            foreach (string communicationServiceName in nodeConfiguration.CommunicationServices)
            {
                CommunicationConfigurationBase communicationConfiguration = (CommunicationConfigurationBase)_configurationService[communicationServiceName];
                IServerCommunicationService serverCommunicationService = _communicationServicesFactory.GetInstance(communicationConfiguration.CommunicationServiceName);
                serverCommunicationService.Init(new SocketListenerSettings(communicationConfiguration.MaxConnections, communicationConfiguration.ReceiveBufferSize, new IPEndPoint(IPAddress.Any, communicationConfiguration.ListeningPort)));
                _communicationServicesRegistry.RegisterInstance(serverCommunicationService, communicationConfiguration.CommunicationServiceName);
            }

            _packetsHandler.Initialize(_cancellationTokenSource.Token);
        }

        private void InitializeModules(CancellationToken ct)
        {
            IEnumerable<IModule> modules = _modulesRepository.GetBulkInstances();

            foreach (IModule module in modules)
            {
                try
                {
                    module.Initialize(ct);
                }
                catch (Exception ex)
                {
                    _log.Error($"Failed to initialize Module '{module.Name}'", ex);
                }
            }
        }

        private void ObtainConfiguredModules()
        {
            INodeConfiguration nodeConfiguration = _configurationService.Get<INodeConfiguration>();

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

        internal void Start()
        {
            INodeConfiguration nodeConfiguration = _configurationService.Get<INodeConfiguration>();
            foreach (string communicationServiceName in nodeConfiguration.CommunicationServices)
            {
                CommunicationConfigurationBase communicationConfiguration = (CommunicationConfigurationBase)_configurationService[communicationServiceName];
                _communicationServicesRegistry.GetInstance(communicationConfiguration.CommunicationServiceName).Start();
            }

            _packetsHandler.Start();

            foreach (IModule module in _modulesRepository.GetBulkInstances())
            {
                module.Start();
            }
        }
    }
}
