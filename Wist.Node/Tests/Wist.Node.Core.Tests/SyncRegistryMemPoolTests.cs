using Chaos.NaCl;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Serializers;
using Wist.Blockchain.Core.Serializers.Signed.Registry;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.Models;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Crypto.ConfidentialAssets;
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
            byte[] powHash = BinaryHelper.GetPowHash(1234);
            IHashCalculation hashCalculationTransactionKey = new MurMurHashCalculation();

            IHashCalculation hashCalculationDefault = new Keccak256HashCalculation();
            IHashCalculation hashCalculationMurMur = new MurMurHashCalculation();
            ISerializersFactory serializersFactory = Substitute.For<ISerializersFactory>();
            IHashCalculationsRepository hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
            IIdentityKeyProvider identityKeyProviderTransactionKey = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            ISigningService signingService = GetRandomCryptoService();
            ILoggerService loggerService = Substitute.For<ILoggerService>();
            IStatesRepository statesRepository = Substitute.For<IStatesRepository>();
            ISynchronizationContext synchronizationContext = new Wist.Core.Synchronization.SynchronizationContext(loggerService);
            statesRepository.GetInstance<ISynchronizationContext>().ReturnsForAnyArgs(synchronizationContext);

            identityKeyProviderTransactionKey.GetKey(null).ReturnsForAnyArgs(c => new Key16(c.ArgAt<Memory<byte>>(0)));

            identityKeyProvidersRegistry.GetInstance("DefaultHash").Returns(new DefaultHashKeyProvider());
            identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider().Returns(identityKeyProviderTransactionKey);

            hashCalculationsRepository.Create(HashType.Keccak256).Returns(hashCalculationDefault);
            hashCalculationsRepository.Create(HashType.MurMur).Returns(hashCalculationMurMur);

            serializersFactory.Create(null).ReturnsForAnyArgs(c =>
            {
                RegistryShortBlockSerializer registryShortBlockSerializer = new RegistryShortBlockSerializer();
                registryShortBlockSerializer.Initialize(c.Arg<SignedPacketBase>());
                return registryShortBlockSerializer;
            });

            SyncRegistryMemPool syncRegistryMemPool = new SyncRegistryMemPool(loggerService, hashCalculationsRepository);

            for (int i = 0; i < fullBlockCount; i++)
            {
                ISigningService signingService1 = GetRandomCryptoService();
                ushort expectedCount = 1000;

                SortedList<ushort, RegistryRegisterBlock> transactionHeaders = GetTransactionHeaders(syncBlockHeight, blockHeight, nonce, expectedCount);
                WitnessStateKey[] transactionHeaderKeys = GetTransactionHeaderKeys(transactionHeaders);

                RegistryShortBlock registryShortBlock = new RegistryShortBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    BlockHeight = blockHeight,
                    Nonce = nonce,
                    PowHash = powHash,
                    WitnessStateKeys = transactionHeaderKeys
                };

                RegistryShortBlockSerializer registryShortBlockSerializer = new RegistryShortBlockSerializer();
                registryShortBlockSerializer.Initialize(registryShortBlock);
                registryShortBlockSerializer.SerializeBody();
                signingService1.Sign(registryShortBlock);
                registryShortBlockSerializer.SerializeFully();

                RegistryFullBlock registryFullBlock = new RegistryFullBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    BlockHeight = blockHeight,
                    Nonce = nonce,
                    PowHash = powHash,
                    StateWitnesses = transactionHeaders.Values.ToArray(),
                    ShortBlockHash = hashCalculationDefault.CalculateHash(registryShortBlock.RawData)
                };

                RegistryFullBlockSerializer serializer = new RegistryFullBlockSerializer();
                serializer.Initialize(registryFullBlock);
                serializer.SerializeBody();
                signingService.Sign(registryFullBlock);
                serializer.SerializeFully();

                registryFullBlocks.Add(registryFullBlock);
                registryShortBlocks.Add(registryShortBlock);
            }

            foreach (RegistryFullBlock fullBlock in registryFullBlocks)
            {
                syncRegistryMemPool.AddCandidateBlock(fullBlock);
            }

            IKey expectedMostConfidentKey = votesPerShortBlockKey.OrderByDescending(kv => kv.Value).Select(kv => kv.Key).First();

            IEnumerable<RegistryFullBlock> actualFullBlocks = syncRegistryMemPool.GetRegistryBlocks();
        }

        private static WitnessStateKey[] GetTransactionHeaderKeys(SortedList<ushort, RegistryRegisterBlock> transactionHeaders)
        {
            WitnessStateKey[] transactionHeaderKeys = new WitnessStateKey[transactionHeaders.Count];

            foreach (ushort order in transactionHeaders.Keys)
            {
                RegistryRegisterBlock registryRegisterBlock = transactionHeaders[order];
                WitnessStateKey key = new WitnessStateKey { PublicKey = registryRegisterBlock.Signer, Height = registryRegisterBlock.BlockHeight };

                transactionHeaderKeys[order] = key;
            }

            return transactionHeaderKeys;
        }

        private static SortedList<ushort, RegistryRegisterBlock> GetTransactionHeaders(ulong syncBlockHeight, ulong blockHeight, uint nonce, ushort expectedCount)
        {
            PacketType expectedReferencedPacketType = PacketType.Transactional;
            ushort expectedReferencedBlockType = BlockTypes.Transaction_TransferFunds;

            SortedList<ushort, RegistryRegisterBlock> transactionHeaders = new SortedList<ushort, RegistryRegisterBlock>(expectedCount);
            for (ushort j = 0; j < expectedCount; j++)
            {
                RegistryRegisterBlock registryRegisterBlock = new RegistryRegisterBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    Nonce = nonce + j,
                    PowHash = BinaryHelper.GetPowHash(1234 + j),
                    BlockHeight = blockHeight,
                    ReferencedPacketType = expectedReferencedPacketType,
                    ReferencedBlockType = expectedReferencedBlockType,
                    ReferencedBodyHash = BinaryHelper.GetDefaultHash(473826643 + j),
                    ReferencedTarget = BinaryHelper.GetDefaultHash(BinaryHelper.GetRandomPublicKey())
                };

                ISigningService signingService = GetRandomCryptoService();

                IIdentityKeyProvider transactionIdentityKeyProvider = Substitute.For<IIdentityKeyProvider>();
                transactionIdentityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key16(c.ArgAt<Memory<byte>>(0)));
                IIdentityKeyProvidersRegistry transactionIdentityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
                transactionIdentityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider().Returns(transactionIdentityKeyProvider);

                IHashCalculationsRepository transactionHashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
                transactionHashCalculationsRepository.Create(HashType.MurMur).Returns(new MurMurHashCalculation());

                RegistryRegisterBlockSerializer serializer1 = new RegistryRegisterBlockSerializer();
                serializer1.Initialize(registryRegisterBlock);
                serializer1.SerializeBody();
                signingService.Sign(registryRegisterBlock);
                serializer1.SerializeFully();

                transactionHeaders.Add(j, registryRegisterBlock);
            }

            return transactionHeaders;
        }

        private static ISigningService GetRandomCryptoService()
        {
            ISigningService signingService = Substitute.For<ISigningService>();
            byte[] privateKey = ConfidentialAssetsHelper.GetRandomSeed();
            byte[] expandedPrivateKey;
            byte[] publicKey;
            Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, privateKey);
            signingService.WhenForAnyArgs(s => s.Sign(null, null)).Do(c => 
            {
                ((SignedPacketBase)c.ArgAt<IPacket>(0)).Signer = new Key32() { Value = publicKey };
                ((SignedPacketBase)c.ArgAt<IPacket>(0)).Signature = Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey);
            });
            signingService.PublicKeys.Returns(new IKey[] { new Key32() { Value = publicKey } });
            return signingService;
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
