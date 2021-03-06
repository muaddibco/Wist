﻿using Chaos.NaCl;
using System.Diagnostics;
using System.Numerics;
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
using Wist.Blockchain.Core.Serializers;
using Wist.Core.Modularity;
using Wist.Core;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistrationLoadModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IHashCalculation _proofOfWorkCalculation;

        public TransactionRegistrationLoadModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ISigningService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository, IHashCalculationsRepository proofOfWorkCalculationRepository, IHashCalculationsRepository hashCalculationRepository)
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository, hashCalculationRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _proofOfWorkCalculation = proofOfWorkCalculationRepository.Create(Globals.POW_TYPE);
        }

        public override string Name => nameof(TransactionRegistrationLoadModule);

        public override void Start()
        {
            byte[] seedTarget = GetRandomSeed();
            byte[] targetKeyBytes = Ed25519.PublicKeyFromSeed(seedTarget);
            byte[] targetHash = CryptoHelper.ComputeHash(targetKeyBytes);

            //IHash hash = HashFactory.Crypto.CreateTiger_4_192();

            //List<byte[]> seeds = new List<byte[]>();
            //List<byte[]> hashes = new List<byte[]>();

            //byte[] seed1 = GetRandomSeed();
            //BigInteger bigInteger1 = new BigInteger(seed1);

            //bool found = false;


            //Stopwatch stopwatch1 = Stopwatch.StartNew();
            //for (int i = 0; i < 10000000; i++)
            //{
            //    bigInteger1 += 1;

            //    byte[] calc = hash.ComputeBytes(bigInteger1.ToByteArray()).GetBytes();

            //    if(calc[0] == 0 && calc[1] == 0 && calc[2] == 0 )
            //    {
            //        found = true;
            //        break;
            //    }
            //}

            //long elapsed = stopwatch1.ElapsedMilliseconds;

            ulong index = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;
            byte[] syncHash = _synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE];

            BigInteger bigInteger = new BigInteger(syncHash);
            ulong nonce = 1234;
            bigInteger += nonce;
            byte[] hashNonce = bigInteger.ToByteArray();
            byte[] powHash = _proofOfWorkCalculation.CalculateHash(hashNonce);
            ulong count = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
                {
                    SyncBlockHeight = index,
                    BlockHeight = count,
                    Signer = _key,
                    Nonce = 1234,
                    PowHash = powHash,
                    ReferencedPacketType = PacketType.Transactional,
                    ReferencedBlockType = BlockTypes.Transaction_TransferFunds,
                    ReferencedBodyHash = new byte[Globals.DEFAULT_HASH_SIZE],
                    ReferencedTarget = targetHash
                };

                ISerializer signatureSupportSerializer = _serializersFactory.Create(transactionRegisterBlock);
                _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);

                _loadCountersService.SentMessages.Increment();
                count++;
            } while (!_cancellationToken.IsCancellationRequested);
        }

        protected override void InitializeInner()
        {
            base.InitializeInner();

        }
    }
}
