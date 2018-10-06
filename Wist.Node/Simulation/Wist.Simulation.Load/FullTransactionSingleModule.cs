using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;
using Wist.Network.Interfaces;
using Wist.Node.Core.Common;
using Wist.Proto.Model;
using Wist.Core.ExtensionMethods;
using Wist.BlockLattice.Core.DataModel.Transactional;

namespace Wist.Simulation.Load
{

    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class FullTransactionSingleModule : LoadModuleBase
    {
        public FullTransactionSingleModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, 
            IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, 
            ICryptoService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IHashCalculationsRepository hashCalculationRepository) 
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository, hashCalculationRepository)
        {
        }

        public override string Name => "FullTransactionSingle";

        public override void Start()
        {
            Channel channel = new Channel("127.0.0.1", 5050, ChannelCredentials.Insecure);
            SyncManager.SyncManagerClient syncManagerClient = new SyncManager.SyncManagerClient(channel);
            
            LastSyncBlock lastSyncBlock = syncManagerClient.GetLastSyncBlock(new Empty());

            byte[] syncHash = lastSyncBlock.Hash.ToByteArray();
            uint nonce = 1111;
            byte[] powHash = GetPowHash(syncHash, nonce);
            byte[] targetAddress = GetRandomTargetAddress();

            ulong blockHeight = 1;

            TransferFundsBlock transferFundsBlock = new TransferFundsBlock
            {
                SyncBlockHeight = lastSyncBlock.Height,
                BlockHeight = blockHeight,
                Nonce = nonce,
                PowHash = powHash,
                HashPrev = new byte[Globals.DEFAULT_HASH_SIZE],
                TargetOriginalHash = targetAddress,
                UptodateFunds = 1000
            };

            ISignatureSupportSerializer transferFundsSerializer = _signatureSupportSerializersFactory.Create(transferFundsBlock);
            transferFundsSerializer.FillBodyAndRowBytes();

            RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
            {
                SyncBlockHeight = lastSyncBlock.Height,
                BlockHeight = blockHeight,
                Nonce = nonce,
                PowHash = powHash,
                TransactionHeader = new TransactionHeader
                {
                    ReferencedPacketType = PacketType.TransactionalChain,
                    ReferencedBlockType = BlockTypes.Transaction_TransferFunds,
                    ReferencedHeight = blockHeight,
                    ReferencedBodyHash = _hashCalculation.CalculateHash(transferFundsBlock.NonHeaderBytes),
                    ReferencedTargetHash = targetAddress
                }
            };

            ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(transactionRegisterBlock);

            _log.Info($"Sending message: {signatureSupportSerializer.GetBytes().ToHexString()}");

            _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);
            _communicationService.PostMessage(_keyTarget, transferFundsSerializer);
        }
    }
}
