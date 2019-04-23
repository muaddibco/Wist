using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Core;
using Wist.Core.HashCalculations;
using Wist.Core.Logging;
using Wist.Core.Models;
using Wist.Core.Synchronization;
using Wist.Proto.Model;

namespace Wist.Node.Core.Interaction
{
    public class SyncManagerImpl : SyncManager.SyncManagerBase
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ILogger _logger;
        private readonly IChainDataService _syncChainDataService;
        private readonly IChainDataService _registryChainDataService;
        private readonly IChainDataService _transactionalDataService;
        private readonly IChainDataService _utxoConfidentialDataService;
        private readonly IHashCalculation _hashCalculation;

        public SyncManagerImpl(ISynchronizationContext synchronizationContext, IChainDataServicesManager chainDataServicesManager, IHashCalculationsRepository hashCalculationsRepository, ILogger logger)
        {
            _synchronizationContext = synchronizationContext;
            _logger = logger;
            _syncChainDataService = chainDataServicesManager.GetChainDataService(PacketType.Synchronization);
            _registryChainDataService = chainDataServicesManager.GetChainDataService(PacketType.Registry);
            _transactionalDataService = chainDataServicesManager.GetChainDataService(PacketType.Transactional);
            _utxoConfidentialDataService = chainDataServicesManager.GetChainDataService(PacketType.UtxoConfidential);
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override Task<SyncBlockDescriptor> GetLastSyncBlock(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new SyncBlockDescriptor
            {
                Height = _synchronizationContext?.LastBlockDescriptor.BlockHeight ?? 0,
                Hash = ByteString.CopyFrom(_synchronizationContext?.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE])
            });
        }

        public override async Task GetDeltaSyncBlocks(ByHeightRequest request, IServerStreamWriter<SyncBlockDescriptor> responseStream, ServerCallContext context)
        {
            IEnumerable<PacketBase> blocks = _syncChainDataService.GetAll(new BlockTypeLowHeightKey(BlockTypes.Synchronization_ConfirmedBlock, request.Height));

            foreach (PacketBase block in blocks)
            {
                await responseStream.WriteAsync(new SyncBlockDescriptor
                {
                    Height = ((SynchronizationConfirmedBlock)block).SyncBlockHeight,
                    Hash = ByteString.CopyFrom(_hashCalculation.CalculateHash(((SynchronizationConfirmedBlock)block).RawData)) //TODO: !!! need to change hash calculation in place to reading from database, otherwise DoS attack is allowed
                });
            }
        }

        public override async Task GetCombinedRegistryBlocksInfoSinceHeight(ByHeightRequest request, IServerStreamWriter<CombinedRegistryBlockInfo> responseStream, ServerCallContext context)
        {
            IEnumerable<PacketBase> blocks = _syncChainDataService.GetAll(new BlockTypeLowHeightKey(BlockTypes.Synchronization_RegistryCombinationBlock, request.Height));
            foreach (PacketBase blockBase in blocks)
            {
                SynchronizationRegistryCombinedBlock registryCombinedBlock = blockBase as SynchronizationRegistryCombinedBlock;
                CombinedRegistryBlockInfo combinedRegistryBlockInfo = new CombinedRegistryBlockInfo
                {
                    SyncBlockHeight = registryCombinedBlock.SyncBlockHeight,
                    Height = registryCombinedBlock.BlockHeight,
                    CombinedRegistryBlocksCount = (uint)registryCombinedBlock.BlockHashes.Length,
                };

                foreach (byte[] hash in registryCombinedBlock.BlockHashes)
                {
                    RegistryFullBlock registryFullBlock = (RegistryFullBlock)_registryChainDataService.Get(new SyncHashKey(registryCombinedBlock.SyncBlockHeight, hash));

                    if(registryFullBlock == null)
                    {
                        registryFullBlock = (RegistryFullBlock)_registryChainDataService.Get(new SyncHashKey(registryCombinedBlock.SyncBlockHeight - 1, hash));
                    }

                    if (registryFullBlock != null)
                    {
                        combinedRegistryBlockInfo.BlockDescriptors.Add(
                            new FullBlockDescriptor
                            {
                                SyncBlockHeight = registryFullBlock.SyncBlockHeight,
                                Round = registryFullBlock.BlockHeight,
                                TransactionsCount = (uint)(registryFullBlock.StateWitnesses.Length + registryFullBlock.UtxoWitnesses.Length),
                                BlockHash = ByteString.CopyFrom(hash)
                            });
                    }
                }

                await responseStream.WriteAsync(combinedRegistryBlockInfo);
            }
        }

        public override async Task GetCombinedRegistryBlocksContentSinceHeight(ByHeightRequest request, IServerStreamWriter<TransactionInfo> responseStream, ServerCallContext context)
        {
            IEnumerable<PacketBase> blocks = _syncChainDataService.GetAll(new BlockTypeLowHeightKey(BlockTypes.Synchronization_RegistryCombinationBlock, request.Height));
            foreach (PacketBase blockBase in blocks)
            {
                SynchronizationRegistryCombinedBlock registryCombinedBlock = blockBase as SynchronizationRegistryCombinedBlock;
                TransactionInfo blockInfo = new TransactionInfo
                {
                    SyncBlockHeight = registryCombinedBlock.SyncBlockHeight,
                    BlockType = registryCombinedBlock.BlockType,
                    PacketType = (uint)registryCombinedBlock.PacketType,
                    Content = ByteString.CopyFrom(registryCombinedBlock.RawData.ToArray())
                };

                await responseStream.WriteAsync(blockInfo);
            }
        }

        public override Task GetAllCombinedRegistryBlocksSinceSync(ByHeightRequest request, IServerStreamWriter<CombinedRegistryBlockInfo> responseStream, ServerCallContext context)
        {
            return Task.Run(() => 
            {
                IEnumerable<PacketBase> blocks = _syncChainDataService.GetAllLastBlocksByType(BlockTypes.Synchronization_RegistryCombinationBlock).Where(b => ((SynchronizationRegistryCombinedBlock)b).SyncBlockHeight > request.Height);
                foreach (PacketBase blockBase in blocks)
                {
                    SynchronizationRegistryCombinedBlock registryCombinedBlock = blockBase as SynchronizationRegistryCombinedBlock;
                    CombinedRegistryBlockInfo combinedRegistryBlockInfo = new CombinedRegistryBlockInfo
                    {
                        SyncBlockHeight = registryCombinedBlock.SyncBlockHeight,
                        Height = registryCombinedBlock.BlockHeight,
                        CombinedRegistryBlocksCount = (uint)registryCombinedBlock.BlockHashes.Length,
                    };

                    foreach (byte[] item in registryCombinedBlock.BlockHashes)
                    {
                        combinedRegistryBlockInfo.BlockDescriptors.Add(new FullBlockDescriptor { BlockHash = ByteString.CopyFrom(item) });
                    }

                    responseStream.WriteAsync(combinedRegistryBlockInfo);
                }
            });
        }

        public override Task GetAllCombinedRegistryBlocksPerSync(ByHeightRequest request, IServerStreamWriter<CombinedRegistryBlockInfo> responseStream, ServerCallContext context)
        {
            return Task.Run(() =>
            {
                IEnumerable<PacketBase> blocks = _syncChainDataService.GetAllLastBlocksByType(BlockTypes.Synchronization_RegistryCombinationBlock).Where(b => ((SynchronizationRegistryCombinedBlock)b).SyncBlockHeight == request.Height);
                foreach (PacketBase blockBase in blocks)
                {
                    SynchronizationRegistryCombinedBlock registryCombinedBlock = blockBase as SynchronizationRegistryCombinedBlock;
                    CombinedRegistryBlockInfo combinedRegistryBlockInfo = new CombinedRegistryBlockInfo
                    {
                        SyncBlockHeight = registryCombinedBlock.SyncBlockHeight,
                        Height = registryCombinedBlock.BlockHeight,
                        CombinedRegistryBlocksCount = (uint)registryCombinedBlock.BlockHashes.Length,
                    };

                    foreach (byte[] item in registryCombinedBlock.BlockHashes)
                    {
                        combinedRegistryBlockInfo.BlockDescriptors.Add(new FullBlockDescriptor { BlockHash = ByteString.CopyFrom(item) });
                    }

                    responseStream.WriteAsync(combinedRegistryBlockInfo);
                }
            });
        }

        public override Task GetTransactionRegistryBlockInfos(FullBlockRequest request, IServerStreamWriter<TransactionRegistryBlockInfo> responseStream, ServerCallContext context)
        {
            return Task.Run(async () => 
            {
                RegistryFullBlock registryFullBlock = (RegistryFullBlock)_registryChainDataService.Get(new DoubleHeightKey(request.SyncBlockHeight, request.Round));

                foreach (RegistryRegisterBlock transactionWitness in registryFullBlock.StateWitnesses)
                {
                    TransactionRegistryBlockInfo blockInfo = new TransactionRegistryBlockInfo();

                    blockInfo.AccountedHeader = new AccountedTransactionHeaderDescriptor
                    {
                        SyncBlockHeight = transactionWitness.SyncBlockHeight,
                        ReferencedBlockType = transactionWitness.ReferencedBlockType,
                        ReferencedPacketType = (uint)transactionWitness.ReferencedPacketType,
                        ReferencedTarget = ByteString.CopyFrom(transactionWitness.ReferencedTarget),
                        ReferencedHeight = transactionWitness.BlockHeight
                    };

                    await responseStream.WriteAsync(blockInfo);
                }

                foreach (RegistryRegisterUtxoConfidential transactionWitness in registryFullBlock.UtxoWitnesses)
                {
                    TransactionRegistryBlockInfo blockInfo = new TransactionRegistryBlockInfo();

                    blockInfo.UtxoHeader = new UtxoTransactionHeaderDescriptor
                    {
                        SyncBlockHeight = transactionWitness.SyncBlockHeight,
                        ReferencedBlockType = transactionWitness.ReferencedBlockType,
                        ReferencedPacketType = (uint)transactionWitness.ReferencedPacketType,
                        ReferencedTarget = ByteString.CopyFrom(transactionWitness.DestinationKey),
                        ReferencedTransactionKey = ByteString.CopyFrom(transactionWitness.TransactionPublicKey),
                        KeyImage = ByteString.CopyFrom(transactionWitness.KeyImage.Value.ToArray())
                    };

                    await responseStream.WriteAsync(blockInfo);
                }
            });
        }

        public override Task<TransactionInfo> GetFullRegistryBlock(HeightHashRequest request, ServerCallContext context)
        {
            RegistryFullBlock block = (RegistryFullBlock)_registryChainDataService.Get(new SyncHashKey(request.Height, request.Hash.ToByteArray()));

            if (block != null)
            {
                TransactionInfo transactionInfo = new TransactionInfo
                {
                    BlockType = block.BlockType,
                    PacketType = (uint)block.PacketType,
                    SyncBlockHeight = block.SyncBlockHeight,
                    Content = ByteString.CopyFrom(block.RawData.ToArray())
                };

                return Task.FromResult(transactionInfo);
            }

            return Task.FromResult(new TransactionInfo { IsEmpty = true });
        }
    }
}
