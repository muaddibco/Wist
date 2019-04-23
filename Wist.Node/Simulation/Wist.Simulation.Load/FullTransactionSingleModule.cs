using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.Core.Serializers;
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
using Wist.Blockchain.Core.DataModel.Transactional;
using Google.Protobuf;
using Wist.Blockchain.Core.Parsers.Registry;
using CommonServiceLocator;
using Wist.Blockchain.Core.DataModel;
using Wist.Core.Modularity;

namespace Wist.Simulation.Load
{

    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class FullTransactionSingleModule : LoadModuleBase
    {
        public FullTransactionSingleModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, 
            IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            ISerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, 
            ISigningService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IHashCalculationsRepository hashCalculationRepository) 
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository, hashCalculationRepository)
        {
        }

        public override string Name => "FullTransactionSingle";

        public override void Start()
        {
            string cmd = null;
            Channel channel = new Channel("127.0.0.1", 5050, ChannelCredentials.Insecure);
            SyncManager.SyncManagerClient syncManagerClient = new SyncManager.SyncManagerClient(channel);
            TransactionalChainManager.TransactionalChainManagerClient transactionalChainManagerClient = new TransactionalChainManager.TransactionalChainManagerClient(channel);
            TransactionalBlockEssense transactionalBlockEssense = transactionalChainManagerClient.GetLastTransactionalBlock(
                new TransactionalBlockRequest { PublicKey = ByteString.CopyFrom(_key.Value.ToArray()) });

            ulong blockHeight = transactionalBlockEssense.Height + 1;
            ulong uptodateFunds = transactionalBlockEssense.UpToDateFunds > 0 ? transactionalBlockEssense.UpToDateFunds : 100000;

            do
            {
                SyncBlockDescriptor lastSyncBlock = syncManagerClient.GetLastSyncBlock(new Empty());
                uptodateFunds -= 10;
                byte[] syncHash = lastSyncBlock.Hash.ToByteArray();
                uint nonce = 1111;
                byte[] powHash = GetPowHash(syncHash, nonce);
                byte[] targetAddress = GetRandomTargetAddress();

                TransferFundsBlock transferFundsBlock = new TransferFundsBlock
                {
                    SyncBlockHeight = lastSyncBlock.Height,
                    BlockHeight = blockHeight,
                    Nonce = nonce,
                    PowHash = powHash,
                    TargetOriginalHash = targetAddress,
                    UptodateFunds = uptodateFunds
                };

                ISerializer transferFundsSerializer = _serializersFactory.Create(transferFundsBlock);
                transferFundsSerializer.SerializeFully();

                RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
                {
                    SyncBlockHeight = lastSyncBlock.Height,
                    BlockHeight = blockHeight,
                    Nonce = nonce,
                    PowHash = powHash,
                    ReferencedPacketType = PacketType.Transactional,
                    ReferencedBlockType = BlockTypes.Transaction_TransferFunds,
                    ReferencedBodyHash = _hashCalculation.CalculateHash(transferFundsBlock.RawData),
                    ReferencedTarget = targetAddress
                };

                ISerializer transactionRegisterBlockSerializer = _serializersFactory.Create(transactionRegisterBlock);

                _log.Info($"Sending message: {transactionRegisterBlockSerializer.GetBytes().ToHexString()}");

                _communicationService.PostMessage(_keyTarget, transactionRegisterBlockSerializer);
                _communicationService.PostMessage(_keyTarget, transferFundsSerializer);

                Console.WriteLine("Block sent. Press <Enter> for next or type 'exit' and press <Enter> for exit...");
                cmd = Console.ReadLine();

                blockHeight++;
            } while (!_cancellationToken.IsCancellationRequested && cmd != "exit");
        }
    }
}
