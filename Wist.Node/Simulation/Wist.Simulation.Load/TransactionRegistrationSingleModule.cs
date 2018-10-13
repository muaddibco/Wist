using System;
using System.Numerics;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
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
using Chaos.NaCl;
using Wist.BlockLattice.Core.Serializers;
using Wist.Node.Core.Common;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistrationSingleModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;

        public TransactionRegistrationSingleModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ICryptoService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository, IHashCalculationsRepository hashCalculationRepository)
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
                    TransactionHeader = new TransactionHeader
                    {
                        ReferencedPacketType = PacketType.Transactional,
                        ReferencedBlockType = BlockTypes.Transaction_TransferFunds,
                        ReferencedHeight = 1234,
                        ReferencedBodyHash = new byte[Globals.DEFAULT_HASH_SIZE],
                        ReferencedTargetHash = targetAddress
                    }
                };

                ISerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionRegisterBlock);

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
