using Chaos.NaCl;
using System.Security.Cryptography;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.Core.Serializers;
using Wist.Network.Interfaces;
using Wist.Network.Communication;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.ExtensionMethods;
using Wist.Core.PerformanceCounters;
using Wist.Simulation.Load.Configuration;
using Wist.Simulation.Load.PerformanceCounters;
using System.Numerics;
using System.Linq;
using Wist.Core.Modularity;
using Wist.Crypto.ConfidentialAssets;
using Wist.Core;

namespace Wist.Simulation.Load
{
    public abstract class LoadModuleBase : ModuleBase
    {
        protected readonly ICommunicationService _communicationService;
        protected readonly IConfigurationService _configurationService;
        protected readonly ISerializersFactory _serializersFactory;
        protected readonly INodesDataService _nodesDataService;
        protected readonly ISigningService _cryptoService;
        protected readonly IIdentityKeyProvider _identityKeyProvider;
        protected readonly IHashCalculation _hashCalculation;
        protected readonly LoadCountersService _loadCountersService;
        protected readonly IHashCalculation _proofOfWorkCalculation;

        protected ICommunicationChannel _communicationChannel;
        protected IKey _key;
        protected IKey _keyTarget;

        public LoadModuleBase(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, 
            IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            ISerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ISigningService cryptoService, 
            IPerformanceCountersRepository performanceCountersRepository, IHashCalculationsRepository hashCalculationRepository)
            : base(loggerService)
        {
            _communicationService = clientCommunicationServiceRepository.GetInstance(nameof(TcpClientCommunicationService));
            _configurationService = configurationService;
            _serializersFactory = signatureSupportSerializersFactory;
            _nodesDataService = nodesDataService;
            _cryptoService = cryptoService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _loadCountersService = performanceCountersRepository.GetInstance<LoadCountersService>();
            _hashCalculation = hashCalculationRepository.Create(Globals.DEFAULT_HASH);
            _proofOfWorkCalculation = hashCalculationRepository.Create(Globals.POW_TYPE);
        }

        protected override void InitializeInner()
        {
            Blockchain.Core.DataModel.Nodes.Node node = _nodesDataService.GetAll().FirstOrDefault();

            if (node != null)
            {
                _keyTarget = node.Key;
            }
            else
            {

                byte[] seedTarget = GetRandomSeed();
                byte[] targetKeyBytes = Ed25519.PublicKeyFromSeed(seedTarget);
                _keyTarget = _identityKeyProvider.GetKey(targetKeyBytes);

                node = new Blockchain.Core.DataModel.Nodes.Node { Key = _keyTarget, IPAddress = System.Net.IPAddress.Parse("127.0.0.1") };

                _nodesDataService.Add(node);
            }

            ClientTcpCommunicationConfiguration clientTcpCommunicationConfiguration = _configurationService.Get<ClientTcpCommunicationConfiguration>();
            _communicationService.Init(new SocketSettings(clientTcpCommunicationConfiguration.MaxConnections, clientTcpCommunicationConfiguration.ReceiveBufferSize, clientTcpCommunicationConfiguration.ListeningPort, System.Net.Sockets.AddressFamily.InterNetwork));
            _communicationService.Start();

            _key = _cryptoService.PublicKeys[0];
        }

        public static byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }

        protected byte[] GetRandomTargetAddress()
        {
            byte[] seedTarget = "1F0B7DBB567EFC99060EC69DD60130B4364E36B7A88248DD234285B3860F63C3".HexStringToByteArray();// GetRandomSeed();
            byte[] targetKeyBytes = ConfidentialAssetsHelper.GetPublicKeyFromSeed(seedTarget);
            byte[] targetHash = _hashCalculation.CalculateHash(targetKeyBytes);
            return targetHash;
        }

        protected byte[] GetPowHash(byte[] hash, ulong nonce)
        {
            BigInteger bigInteger = new BigInteger(hash);
            bigInteger += nonce;
            byte[] hashNonce = bigInteger.ToByteArray();
            byte[] powHash = _proofOfWorkCalculation.CalculateHash(hashNonce);
            return powHash;
        }
    }
}
