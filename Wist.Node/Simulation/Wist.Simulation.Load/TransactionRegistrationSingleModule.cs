using System;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;
using Wist.Core.HashCalculations;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Core.ExtensionMethods;
using Wist.Blockchain.Core.Serializers;
using Wist.Core.Modularity;
using Wist.Core;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistrationSingleModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;

        public TransactionRegistrationSingleModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ISigningService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository, IHashCalculationsRepository hashCalculationRepository)
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository, hashCalculationRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
        }

        public override string Name => nameof(TransactionRegistrationSingleModule);

        public override void Start()
        {
            ulong index = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;
            byte[] syncHash = _synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE];
            string cmd = null;

            byte[] powHash = GetPowHash(syncHash, 1234);

            byte[] targetAddress = GetRandomTargetAddress();

            do
            {
                RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
                {
                    SyncBlockHeight = index,
                    BlockHeight = 1,
                    Signer = _key,
                    Nonce = 1234,
                    PowHash = powHash,
                    ReferencedPacketType = PacketType.Transactional,
                    ReferencedBlockType = BlockTypes.Transaction_TransferFunds,
                    ReferencedBodyHash = new byte[Globals.DEFAULT_HASH_SIZE],
                    ReferencedTarget = targetAddress
                };

                ISerializer signatureSupportSerializer = _serializersFactory.Create(transactionRegisterBlock);

                _log.Info($"Sending message: {signatureSupportSerializer.GetBytes().ToHexString()}");

                _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);

                if (_cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine("Block sent. Press <Enter> for next or type 'exit' and press <Enter> for exit...");
                cmd = Console.ReadLine();
            } while (!_cancellationToken.IsCancellationRequested && cmd != "exit");
        }

        protected override void InitializeInner()
        {
            base.InitializeInner();

        }
    }
}
