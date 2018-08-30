using Chaos.NaCl;
using System.Security.Cryptography;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;
using Wist.Node.Core.Modules;
using Wist.Simulation.Load.Configuration;
using Wist.Simulation.Load.PerformanceCounters;

namespace Wist.Simulation.Load
{
    public abstract class LoadModuleBase : ModuleBase
    {
        protected readonly ICommunicationService _communicationService;
        protected readonly IConfigurationService _configurationService;
        protected readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        protected readonly INodesDataService _nodesDataService;
        protected readonly ICryptoService _cryptoService;
        protected readonly IIdentityKeyProvider _identityKeyProvider;
        protected readonly IHashCalculation _hashCalculation;
        protected readonly LoadCountersService _loadCountersService;

        protected ICommunicationChannel _communicationChannel;
        protected IKey _key;
        protected IKey _keyTarget;

        public LoadModuleBase(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ICryptoService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IHashCalculationsRepository hashCalculationRepository)
            : base(loggerService)
        {
            _communicationService = clientCommunicationServiceRepository.GetInstance(nameof(TcpClientCommunicationService));
            _configurationService = configurationService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodesDataService = nodesDataService;
            _cryptoService = cryptoService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _loadCountersService = performanceCountersRepository.GetInstance<LoadCountersService>();
            _hashCalculation = hashCalculationRepository.Create(Globals.POW_TYPE);
        }

        protected override void InitializeInner()
        {
            byte[] seedTarget = GetRandomSeed();
            byte[] targetKeyBytes = Ed25519.PublicKeyFromSeed(seedTarget);
            _keyTarget = _identityKeyProvider.GetKey(targetKeyBytes);

            BlockLattice.Core.DataModel.Nodes.Node node = new BlockLattice.Core.DataModel.Nodes.Node { Key = _keyTarget, IPAddress = System.Net.IPAddress.Parse("127.0.0.1") };

            _nodesDataService.Add(node);

            ClientTcpCommunicationConfiguration clientTcpCommunicationConfiguration = _configurationService.Get<ClientTcpCommunicationConfiguration>();
            _communicationService.Init(new SocketSettings(clientTcpCommunicationConfiguration.MaxConnections, clientTcpCommunicationConfiguration.ReceiveBufferSize, clientTcpCommunicationConfiguration.ListeningPort, System.Net.Sockets.AddressFamily.InterNetwork));
            _communicationService.Start();

            byte[] seed = GetRandomSeed();

            _cryptoService.Initialize(seed);

            byte[] keyBytes = Ed25519.PublicKeyFromSeed(seed);
            _key = _identityKeyProvider.GetKey(keyBytes);
        }

        protected byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }
    }
}
