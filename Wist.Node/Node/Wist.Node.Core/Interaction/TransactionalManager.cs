using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DAL.Keys;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Core;
using Wist.Core.ExtensionMethods;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.Models;
using Wist.Proto.Model;

namespace Wist.Node.Core.Interaction
{
    public class TransactionalChainManagerImpl : TransactionalChainManager.TransactionalChainManagerBase
    {
        private readonly ILogger _logger;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IChainDataService _registryChainDataService;
        private readonly IChainDataService _transactionalDataService;
        private readonly IChainDataService _utxoConfidentialDataService;
        private readonly IHashCalculation _hashCalculation;

        public TransactionalChainManagerImpl(IChainDataServicesManager chainDataServicesManager, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository, ILogger logger)
        {
            _logger = logger;
            _transactionalDataService = chainDataServicesManager.GetChainDataService(PacketType.Transactional);
            _registryChainDataService = chainDataServicesManager.GetChainDataService(PacketType.Registry);
            _utxoConfidentialDataService = chainDataServicesManager.GetChainDataService(PacketType.UtxoConfidential);
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override Task<TransactionalBlockEssense> GetLastTransactionalBlock(TransactionalBlockRequest request, ServerCallContext context)
        {
            if (request.PublicKey == null)
            {
                throw new ArgumentNullException(nameof(request.PublicKey));
            }

            byte[] keyBytes = request.PublicKey.ToByteArray();

            if (keyBytes.Length != Globals.NODE_PUBLIC_KEY_SIZE)
            {
                throw new ArgumentException($"Public key size must be of {Globals.NODE_PUBLIC_KEY_SIZE} bytes");
            }

            IKey key = _identityKeyProvider.GetKey(keyBytes);
            TransactionalPacketBase transactionalBlockBase = (TransactionalPacketBase)_transactionalDataService.GetLastBlock(key);

            TransactionalBlockEssense transactionalBlockEssense = new TransactionalBlockEssense
            {
                Height = transactionalBlockBase?.BlockHeight ?? 0,
                //TODO: need to reconsider hash calculation here since it is potential point of DoS attack
                Hash = ByteString.CopyFrom(transactionalBlockBase != null ? _hashCalculation.CalculateHash(transactionalBlockBase.RawData) : new byte[Globals.DEFAULT_HASH_SIZE]),
                UpToDateFunds = transactionalBlockBase?.UptodateFunds ?? 0
            };

            return Task.FromResult(transactionalBlockEssense);
        }

        public override Task<TransactionInfo> GetTransactionInfo(TransactionRequest request, ServerCallContext context)
        {
            byte[] hash = request.Hash.ToByteArray();
            try
            {
                PacketBase blockBase = _transactionalDataService.Get(new SyncHashKey(request.SyncBlockHeight, hash));

                if (blockBase != null)
                {
                    return Task.FromResult(new TransactionInfo
                        {
                            SyncBlockHeight = blockBase.SyncBlockHeight,
                            PacketType = (uint)blockBase.PacketType,
                            BlockType = blockBase.BlockType,
                            Content = ByteString.CopyFrom(blockBase.RawData.ToArray())
                        });
                }
                else
                {
                    blockBase = _transactionalDataService.Get(new SyncHashKey(request.SyncBlockHeight - 1, hash));
                    if (blockBase != null)
                    {
                        return Task.FromResult(new TransactionInfo
                        {
                            SyncBlockHeight = blockBase.SyncBlockHeight,
                            PacketType = (uint)blockBase.PacketType,
                            BlockType = blockBase.BlockType,
                            Content = ByteString.CopyFrom(blockBase.RawData.ToArray())
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to retrieve block for SyncBlockHeight {request.SyncBlockHeight} and ReferencedHash {hash.ToHexString()}", ex);
            }

            return Task.FromResult(new TransactionInfo());
        }

        public override Task GetTransactionInfos(FullBlockRequest request, IServerStreamWriter<TransactionInfo> responseStream, ServerCallContext context)
        {
            return Task.Run(async () =>
            {
                RegistryFullBlock registryFullBlock = (RegistryFullBlock)_registryChainDataService.Get(new DoubleHeightKey(request.SyncBlockHeight, request.Round));
                foreach (RegistryRegisterBlock transactionWitness in registryFullBlock.StateWitnesses)
                {
                    try
                    {

                        PacketBase blockBase = _transactionalDataService.Get(new SyncHashKey(transactionWitness.SyncBlockHeight, transactionWitness.ReferencedBodyHash));

                        if (blockBase != null)
                        {
                            await responseStream.WriteAsync(
                                new TransactionInfo
                                {
                                    SyncBlockHeight = transactionWitness.SyncBlockHeight,
                                    PacketType = (uint)transactionWitness.ReferencedPacketType,
                                    BlockType = transactionWitness.ReferencedBlockType,
                                    Content = ByteString.CopyFrom(blockBase.RawData.ToArray())
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to retrieve block for SyncBlockHeight {request.SyncBlockHeight} and Round {request.Round}", ex);
                    }
                }

                foreach (RegistryRegisterUtxoConfidential transactionWitness in registryFullBlock.UtxoWitnesses)
                {
                    try
                    {

                        PacketBase blockBase = _utxoConfidentialDataService.Get(new SyncHashKey(transactionWitness.SyncBlockHeight, transactionWitness.ReferencedBodyHash));

                        if (blockBase != null)
                        {
                            await responseStream.WriteAsync(
                                new TransactionInfo
                                {
                                    SyncBlockHeight = transactionWitness.SyncBlockHeight,
                                    PacketType = (uint)transactionWitness.ReferencedPacketType,
                                    BlockType = transactionWitness.ReferencedBlockType,
                                    Content = ByteString.CopyFrom(blockBase.RawData.ToArray())
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to retrieve block for SyncBlockHeight {request.SyncBlockHeight} and Round {request.Round}", ex);
                    }
                }
            }, context.CancellationToken);
        }
    }
}
