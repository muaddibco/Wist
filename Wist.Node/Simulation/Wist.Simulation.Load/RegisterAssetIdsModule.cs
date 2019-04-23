using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.DataModel.Transactional;
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
using Wist.Core.Modularity;
using Wist.Core.ExtensionMethods;
using Wist.Core.PerformanceCounters;
using Wist.Network.Interfaces;
using Wist.Proto.Model;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class RegisterAssetIdsModule : LoadModuleBase
    {
        public RegisterAssetIdsModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ISigningService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IHashCalculationsRepository hashCalculationRepository) : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository, hashCalculationRepository)
        {
        }

        public override string Name => nameof(RegisterAssetIdsModule);

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
            ulong tagId = 5005;

            byte[][] idCards = new byte[][]
            {
                GetComplementedAssetId(327483038),
                GetComplementedAssetId(327152054),
                GetComplementedAssetId(328051065),
            };

            string[] assetInfos = new string[]
            {
                "Kirill Gandyl",
                "Elena Zaychikov",
                "Etel Gandyl"
            };

            do
            {
                SyncBlockDescriptor lastSyncBlock = syncManagerClient.GetLastSyncBlock(new Empty());
                byte[] syncHash = lastSyncBlock.Hash.ToByteArray();
                uint nonce = 1111;
                byte[] powHash = GetPowHash(syncHash, nonce);
                byte[] targetAddress = new byte[32];

                AssetsIssuanceGroup assetsIssuanceGroup = new AssetsIssuanceGroup
                {
                    AssetIssuances = new AssetIssuance[idCards.Length]                    
                };

                for (int i = 0; i < idCards.Length; i++)
                {
                    assetsIssuanceGroup.AssetIssuances[i] = new AssetIssuance { AssetId = idCards[i], IssuedAssetInfo = assetInfos[i] };
                }

                IssueGroupedAssets issueAssetsBlock = new IssueGroupedAssets
                {
                    SyncBlockHeight = lastSyncBlock.Height,
                    BlockHeight = blockHeight,
                    Nonce = nonce,
                    PowHash = powHash,
                    TagId = tagId,
                    IssuanceInfo = "Issue ID cards",
                    AssetsIssuanceGroups = new AssetsIssuanceGroup[] { assetsIssuanceGroup }
                };

                ISerializer issueAssetsBlockSerializer = _serializersFactory.Create(issueAssetsBlock);
                issueAssetsBlockSerializer.SerializeFully();

                RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
                {
                    SyncBlockHeight = lastSyncBlock.Height,
                    BlockHeight = blockHeight,
                    Nonce = nonce,
                    PowHash = powHash,
                    ReferencedPacketType = PacketType.Transactional,
                    ReferencedBlockType = BlockTypes.Transaction_TransferFunds,
                    ReferencedBodyHash = _hashCalculation.CalculateHash(issueAssetsBlock.RawData),
                    ReferencedTarget = targetAddress
                };

                ISerializer transactionRegisterBlockSerializer = _serializersFactory.Create(transactionRegisterBlock);

                _log.Info($"Sending message: {transactionRegisterBlockSerializer.GetBytes().ToHexString()}");

                _communicationService.PostMessage(_keyTarget, transactionRegisterBlockSerializer);
                _communicationService.PostMessage(_keyTarget, issueAssetsBlockSerializer);

                Console.WriteLine("Block sent. Press <Enter> for next or type 'exit' and press <Enter> for exit...");
                cmd = Console.ReadLine();

                blockHeight++;
            } while (!_cancellationToken.IsCancellationRequested && cmd != "exit");
        }

        private byte[] GetComplementedAssetId(ulong assetId)
        {
            byte[] assetIdBytes = new byte[32];
            Array.Copy(BitConverter.GetBytes(assetId), 0, assetIdBytes, 0, sizeof(ulong));

            return assetIdBytes;
        }
    }
}

