using Chaos.NaCl;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Serializers;
using Wist.BlockLattice.Core.Serializers.Signed.Registry;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Crypto.HashCalculations;
using Wist.Node.Core.Synchronization;
using Wist.Tests.Core;
using Wist.Tests.Core.Fixtures;
using Xunit;

namespace Wist.Node.Core.Tests
{
    public class SyncRegistryMemPoolTests : IClassFixture<DependencyInjectionSupportFixture>
    {
        [Fact]
        public void GetMostConfidentFullBlockTest()
        {
            List<RegistryFullBlock> registryFullBlocks = new List<RegistryFullBlock>();
            List<RegistryShortBlock> registryShortBlocks = new List<RegistryShortBlock>();
            Dictionary<IKey, int> votesPerShortBlockKey = new Dictionary<IKey, int>();

            int fullBlockCount = 10;
            int votersCount = 100;
            ulong syncBlockHeight = 1;
            ulong blockHeight = 12;
            uint nonce = 0;
            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            IHashCalculation hashCalculationTransactionKey = new MurMurHashCalculation();

            IHashCalculation hashCalculationDefault = new Keccak256HashCalculation();
            IHashCalculation hashCalculationMurMur = new MurMurHashCalculation();
            ISerializersFactory signatureSupportSerializersFactory = Substitute.For<ISerializersFactory>();
            IHashCalculationsRepository hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
            IIdentityKeyProvider identityKeyProviderTransactionKey = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            ICryptoService cryptoService = GetRandomCryptoService();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            IStatesRepository statesRepository = Substitute.For<IStatesRepository>();
            ISynchronizationContext synchronizationContext = new Wist.Core.Synchronization.SynchronizationContext(loggerService);
            statesRepository.GetInstance<ISynchronizationContext>().ReturnsForAnyArgs(synchronizationContext);

            identityKeyProviderTransactionKey.GetKey(null).ReturnsForAnyArgs(c => new Key16(c.ArgAt<Memory<byte>>(0)));

            identityKeyProvidersRegistry.GetInstance("DefaultHash").Returns(new DefaultHashKeyProvider());
            identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider().Returns(identityKeyProviderTransactionKey);

            hashCalculationsRepository.Create(HashType.Keccak256).Returns(hashCalculationDefault);
            hashCalculationsRepository.Create(HashType.MurMur).Returns(hashCalculationMurMur);

            signatureSupportSerializersFactory.Create(null).ReturnsForAnyArgs(c =>
            {
                RegistryShortBlockSerializer registryShortBlockSerializer = new RegistryShortBlockSerializer(cryptoService, identityKeyProvidersRegistry, hashCalculationsRepository);
                registryShortBlockSerializer.Initialize(c.Arg<SignedBlockBase>());
                return registryShortBlockSerializer;
            });

            SyncRegistryMemPool syncRegistryMemPool = new SyncRegistryMemPool(signatureSupportSerializersFactory, hashCalculationsRepository, identityKeyProvidersRegistry, cryptoService, statesRepository, loggerService);

            for (int i = 0; i < fullBlockCount; i++)
            {
                ICryptoService cryptoService1 = GetRandomCryptoService();
                ushort expectedCount = 1000;

                SortedList<ushort, RegistryRegisterBlock> transactionHeaders = GetTransactionHeaders(syncBlockHeight, blockHeight, nonce, expectedCount);
                SortedList<ushort, IKey> transactionHeaderKeys = GetTransactionHeaderKeys(hashCalculationTransactionKey, transactionHeaders);

                RegistryShortBlock registryShortBlock = new RegistryShortBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    BlockHeight = blockHeight,
                    Nonce = nonce,
                    PowHash = powHash,
                    TransactionHeaderHashes = transactionHeaderKeys
                };

                RegistryShortBlockSerializer registryShortBlockSerializer = new RegistryShortBlockSerializer(cryptoService1, identityKeyProvidersRegistry, hashCalculationsRepository);
                registryShortBlockSerializer.Initialize(registryShortBlock);
                registryShortBlockSerializer.FillBodyAndRowBytes();

                RegistryFullBlock registryFullBlock = new RegistryFullBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    BlockHeight = blockHeight,
                    Nonce = nonce,
                    PowHash = powHash,
                    TransactionHeaders = transactionHeaders,
                    ShortBlockHash = hashCalculationDefault.CalculateHash(registryShortBlock.BodyBytes)
                };

                RegistryFullBlockSerializer serializer = new RegistryFullBlockSerializer(cryptoService1, identityKeyProvidersRegistry, hashCalculationsRepository);
                serializer.Initialize(registryFullBlock);
                serializer.FillBodyAndRowBytes();

                registryFullBlocks.Add(registryFullBlock);
                registryShortBlocks.Add(registryShortBlock);
            }

            foreach (RegistryFullBlock fullBlock in registryFullBlocks)
            {
                syncRegistryMemPool.AddCandidateBlock(fullBlock);
            }

            Random random = new Random();

            for (int i = 0; i < votersCount; i++)
            {
                ICryptoService cryptoService2 = GetRandomCryptoService();

                foreach (var registryShortBlock in registryShortBlocks)
                {
                    byte[] hashBytes = hashCalculationDefault.CalculateHash(registryShortBlock.BodyBytes);
                    Random randNum = new Random();
                    byte[] bitMask = Enumerable.Repeat(0, registryShortBlock.TransactionHeaderHashes.Count).Select(j => (byte)randNum.Next(0, 255)).ToArray();
                    byte[] expectedProof = Enumerable.Repeat(0, 16).Select(j => (byte)randNum.Next(0, 255)).ToArray();
                    IKey shortBlockKey = new Key32(hashBytes);
                    long vote = GetConfidence(bitMask);
                    if(!votesPerShortBlockKey.ContainsKey(shortBlockKey))
                    {
                        votesPerShortBlockKey.Add(shortBlockKey, (ushort)vote);
                    }
                    else
                    {
                        votesPerShortBlockKey[shortBlockKey] += (ushort)vote;
                    }

                    RegistryConfidenceBlock registryConfidenceBlock = new RegistryConfidenceBlock
                    {
                        SyncBlockHeight = syncBlockHeight,
                        BlockHeight = blockHeight,
                        Nonce = nonce,
                        PowHash = powHash,
                        ReferencedBlockHash = hashBytes,
                        BitMask = bitMask,
                        ConfidenceProof = expectedProof
                    };

                    RegistryConfidenceBlockSerializer registryConfidenceBlockSerializer = new RegistryConfidenceBlockSerializer(cryptoService2, identityKeyProvidersRegistry, hashCalculationsRepository);
                    registryConfidenceBlockSerializer.Initialize(registryConfidenceBlock);
                    registryConfidenceBlockSerializer.FillBodyAndRowBytes();

                    syncRegistryMemPool.AddVotingBlock(registryConfidenceBlock);
                }
            }

            IKey expectedMostConfidentKey = votesPerShortBlockKey.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).First();

            RegistryFullBlock actualFullBlock = syncRegistryMemPool.GetMostConfidentFullBlock(blockHeight);
            IKey actualMostConfidentKey = new Key32(actualFullBlock.ShortBlockHash);

            Assert.Equal(expectedMostConfidentKey, actualMostConfidentKey);
        }

        private static SortedList<ushort, IKey> GetTransactionHeaderKeys(IHashCalculation hashCalculationTransactionKey, SortedList<ushort, RegistryRegisterBlock> transactionHeaders)
        {
            SortedList<ushort, IKey> transactionHeaderKeys = new SortedList<ushort, IKey>(transactionHeaders.Count);

            foreach (ushort order in transactionHeaders.Keys)
            {
                RegistryRegisterBlock registryRegisterBlock = transactionHeaders[order];
                byte[] hashBytes = hashCalculationTransactionKey.CalculateHash(registryRegisterBlock.BodyBytes);
                IKey key = new Key16(hashBytes);

                transactionHeaderKeys.Add(order, key);
            }

            return transactionHeaderKeys;
        }

        private static SortedList<ushort, RegistryRegisterBlock> GetTransactionHeaders(ulong syncBlockHeight, ulong blockHeight, uint nonce, ushort expectedCount)
        {
            PacketType expectedReferencedPacketType = PacketType.Transactional;
            ushort expectedReferencedBlockType = BlockTypes.Transaction_TransferFunds;
            ulong expectedReferencedHeight = 123466774;

            SortedList<ushort, RegistryRegisterBlock> transactionHeaders = new SortedList<ushort, RegistryRegisterBlock>(expectedCount);
            for (ushort j = 0; j < expectedCount; j++)
            {
                RegistryRegisterBlock registryRegisterBlock = new RegistryRegisterBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    Nonce = nonce + j,
                    PowHash = BinaryBuilder.GetPowHash(1234 + j),
                    BlockHeight = blockHeight,
                    TransactionHeader = new TransactionHeader
                    {
                        ReferencedPacketType = expectedReferencedPacketType,
                        ReferencedBlockType = expectedReferencedBlockType,
                        ReferencedHeight = expectedReferencedHeight,
                        ReferencedBodyHash = BinaryBuilder.GetDefaultHash(473826643 + j),
                        ReferencedTargetHash = BinaryBuilder.GetDefaultHash(BinaryBuilder.GetRandomPublicKey())
                    }
                };

                ICryptoService cryptoService = GetRandomCryptoService();

                IIdentityKeyProvider transactionIdentityKeyProvider = Substitute.For<IIdentityKeyProvider>();
                transactionIdentityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key16(c.ArgAt<Memory<byte>>(0)));
                IIdentityKeyProvidersRegistry transactionIdentityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
                transactionIdentityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider().Returns(transactionIdentityKeyProvider);

                IHashCalculationsRepository transactionHashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
                transactionHashCalculationsRepository.Create(HashType.MurMur).Returns(new MurMurHashCalculation());

                RegistryRegisterBlockSerializer serializer1 = new RegistryRegisterBlockSerializer(cryptoService, transactionIdentityKeyProvidersRegistry, transactionHashCalculationsRepository);
                serializer1.Initialize(registryRegisterBlock);
                serializer1.FillBodyAndRowBytes();

                transactionHeaders.Add(j, registryRegisterBlock);
            }

            return transactionHeaders;
        }

        private static ICryptoService GetRandomCryptoService()
        {
            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            byte[] privateKey = CryptoHelper.GetRandomSeed();
            byte[] expandedPrivateKey;
            byte[] publicKey;
            Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, privateKey);
            cryptoService.Sign(null).ReturnsForAnyArgs(c => Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey));
            cryptoService.PublicKey.Returns(new Key32() { Value = publicKey });
            return cryptoService;
        }

        private static long GetConfidence(byte[] bitMask)
        {
            long sum = 0;
            byte[] numBytes = new byte[8];
            for (int i = 0; i < bitMask.Length; i += 8)
            {
                long num;
                if (bitMask.Length - i < 8)
                {
                    numBytes[0] = 0;
                    numBytes[1] = 0;
                    numBytes[2] = 0;
                    numBytes[3] = 0;
                    numBytes[4] = 0;
                    numBytes[5] = 0;
                    numBytes[6] = 0;
                    numBytes[7] = 0;

                    Array.Copy(bitMask, i, numBytes, 0, bitMask.Length - i);
                    num = BitConverter.ToInt64(numBytes, 0);
                }
                else
                {
                    num = BitConverter.ToInt64(bitMask, i);
                }

                sum += NumberOfSetBits(num);
            }

            return sum;
        }

        private static long NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56;
        }
    }
}
