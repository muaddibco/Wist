using System;
using System.Numerics;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
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
using Wist.Node.Core.Interfaces;
using Wist.Core.ExtensionMethods;
using Chaos.NaCl;
using Wist.BlockLattice.Core.Serializers;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistrationSingleModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IHashCalculation _proofOfWorkCalculation;

        public TransactionRegistrationSingleModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ICryptoService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository, IHashCalculationRepository proofOfWorkCalculationRepository, IHashCalculationRepository hashCalculationRepository)
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository, hashCalculationRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _proofOfWorkCalculation = proofOfWorkCalculationRepository.Create(Globals.POW_TYPE);
        }

        public override string Name => nameof(TransactionRegistrationSingleModule);

        public override void Start()
        {
            ulong index = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;
            byte[] syncHash = _synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.HASH_SIZE];
            string cmd = null;

            BigInteger bigInteger = new BigInteger(syncHash);
            ulong nonce = 1234;
            bigInteger += nonce;
            byte[] hashNonce = bigInteger.ToByteArray();
            byte[] powHash = _proofOfWorkCalculation.CalculateHash(hashNonce);


            byte[] seedTarget = GetRandomSeed();
            byte[] targetKeyBytes = Ed25519.PublicKeyFromSeed(seedTarget);
            byte[] targetHash = CryptoHelper.ComputeHash(targetKeyBytes);

            do
            {
                TransactionRegisterBlock transactionRegisterBlock = new TransactionRegisterBlock
                {
                    SyncBlockHeight = index,
                    BlockHeight = 1,
                    Key = _key,
                    Nonce = 1234,
                    HashNonce = powHash,
                    TransactionHeader = new TransactionHeader
                    {
                        ReferencedPacketType = PacketType.TransactionalChain,
                        ReferencedBlockType = BlockTypes.Transaction_Confirm,
                        ReferencedHeight = 1234,
                        ReferencedBodyHash = new byte[Globals.HASH_SIZE],
                        ReferencedTargetHash = targetHash
                    }
                };

                ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionRegisterBlock);

                _log.Info($"Sending message: {signatureSupportSerializer.GetBytes().ToHexString()}");

                _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);

                if(_cancellationToken.IsCancellationRequested)
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
