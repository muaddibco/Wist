using HashLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;
using Wist.Core.ProofOfWork;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Roles;
using Wist.Simulation.Load.PerformanceCounters;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistrationLoadModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IProofOfWorkCalculation _proofOfWorkCalculation;

        public TransactionRegistrationLoadModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ICryptoService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository)
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
            _proofOfWorkCalculation = proofOfWorkCalculationRepository.Create(Globals.POW_TYPE);
        }

        public override string Name => nameof(TransactionRegistrationLoadModule);

        protected override void InitializeInner()
        {
            base.InitializeInner();

            IHash hash = HashFactory.Crypto.CreateTiger_4_192();

            List<byte[]> seeds = new List<byte[]>();
            List<byte[]> hashes = new List<byte[]>();

            byte[] seed1 = GetRandomSeed();
            BigInteger bigInteger1 = new BigInteger(seed1);

            bool found = false;


            Stopwatch stopwatch1 = Stopwatch.StartNew();
            for (int i = 0; i < 10000000; i++)
            {
                bigInteger1 += 1;

                byte[] calc = hash.ComputeBytes(bigInteger1.ToByteArray()).GetBytes();

                if(calc[0] == 0 && calc[1] == 0 && calc[2] == 0 )
                {
                    found = true;
                    break;
                }
            }

            long elapsed = stopwatch1.ElapsedMilliseconds;

            ulong index = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;
            byte[] syncHash = _synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.HASH_SIZE];

            BigInteger bigInteger = new BigInteger(syncHash);
            ulong nonce = 1234;
            bigInteger += nonce;
            byte[] hashNonce = bigInteger.ToByteArray();
            byte[] powHash = _proofOfWorkCalculation.CalculateHash(hashNonce);
            int count = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            do
            {
                TransactionRegisterBlock transactionRegisterBlock = new TransactionRegisterBlock
                {
                    SyncBlockOrder = index,
                    BlockHeight = 1,
                    Key = _key,
                    Nonce = 1234,
                    HashNonce = powHash,
                    POWType = Globals.POW_TYPE,
                    ReferencedPacketType = PacketType.TransactionalChain,
                    ReferencedBlockType = BlockTypes.Transaction_Confirm,
                    ReferencedHeight = 1234,
                    ReferencedBodyHash = new byte[Globals.HASH_SIZE]
                };

                ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionRegisterBlock);
                _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);

                _loadCountersService.SentMessages.Increment();
                count++;
            } while (true);
        }
    }
}
