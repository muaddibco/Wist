﻿using Chaos.NaCl;
using CommonServiceLocator;
using System;
using System.Security.Cryptography;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Node.Core.Configuration;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Roles;
using Wist.Simulation.Load.Configuration;

namespace Wist.Simulation.Load
{

    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SimpleLoadModule : ModuleBase
    {
        private ICommunicationChannel _communicationChannel;
        private readonly ICommunicationService _communicationService;
        private readonly IConfigurationService _configurationService;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly INodesDataService _nodesDataService;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public SimpleLoadModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService) 
            : base(loggerService)
        {
            _communicationService = clientCommunicationServiceRepository.GetInstance(nameof(TcpClientCommunicationService));
            _configurationService = configurationService;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _nodesDataService = nodesDataService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override string Name => nameof(SimpleLoadModule);

        protected override void InitializeInner()
        {
            ClientTcpCommunicationConfiguration clientTcpCommunicationConfiguration = _configurationService.Get<ClientTcpCommunicationConfiguration>();
            _communicationService.Init(new SocketSettings(clientTcpCommunicationConfiguration.MaxConnections, clientTcpCommunicationConfiguration.ReceiveBufferSize, clientTcpCommunicationConfiguration.ListeningPort, System.Net.Sockets.AddressFamily.InterNetwork));

            byte[] seed = GetRandomSeed();
            byte[] keyBytes = Ed25519.PublicKeyFromSeed(seed);
            IKey key = _identityKeyProvider.GetKey(keyBytes);

            byte[] seedTarget = GetRandomSeed();
            byte[] targetKeyBytes = Ed25519.PublicKeyFromSeed(seedTarget);
            IKey keyTarget = _identityKeyProvider.GetKey(targetKeyBytes);

            Wist.BlockLattice.Core.DataModel.Nodes.Node node = new BlockLattice.Core.DataModel.Nodes.Node { Key = keyTarget, IPAddress = System.Net.IPAddress.Parse("127.0.0.1")};

            _nodesDataService.Add(node);


            SynchronizationConfirmedBlock synchronizationConfirmedBlock = new SynchronizationConfirmedBlock()
            {
                BlockHeight = 1,
                Key = key,
                ReportedTime = DateTime.Now,
                Round = 1
            };

            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(synchronizationConfirmedBlock);
            _communicationService.PostMessage(keyTarget, signatureSupportSerializer);
        }

        private byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }
    }
}