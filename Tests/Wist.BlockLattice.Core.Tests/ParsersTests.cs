using Chaos.NaCl;
using HashLib;
using System;
using System.IO;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Parsers.Synchronization;
using Wist.Core.Identity;
using Wist.Core.ExtensionMethods;
using Wist.Tests.Core;
using Wist.Tests.Core.Fixtures;
using Xunit;
using NSubstitute;
using Wist.Core.HashCalculations;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Parsers.Registry;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Cryptography;
using System.Collections.Generic;
using Wist.BlockLattice.Core.Serializers.Signed.Registry;

namespace Wist.BlockLattice.Core.Tests
{
    public class ParsersTests : IClassFixture<DependencyInjectionSupportFixture>
    {
        [Fact]
        public void SynchronizationConfirmedBlockParserTest()
        {
            IIdentityKeyProvider identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();

            identityKeyProvidersRegistry.GetInstance().Returns(identityKeyProvider);
            identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key32() { Value = c.Arg<byte[]>() });

            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            byte signersCount = 10;
            byte[] body = new byte[11 + Globals.NODE_PUBLIC_KEY_SIZE * signersCount + Globals.SIGNATURE_SIZE * signersCount];
            ushort round = 1;
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] publicKey = Ed25519.PublicKeyFromSeed(privateKey);
            byte[] expectedSignature;

            byte[][] expectedSignerPKs = new byte[signersCount][];
            byte[][] expectedSignerSignatures = new byte[signersCount][];

            DateTime expectedDateTime = DateTime.Now;

            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            byte[] prevHash = BinaryBuilder.GetDefaultHash(1234);

            for (int i = 0; i < signersCount; i++)
            {
                byte[] privateSignerKey = BinaryBuilder.GetRandomSeed();
                byte[] publicSignerKey;
                byte[] expandedSignerKey;

                Ed25519.KeyPairFromSeed(out publicSignerKey, out expandedSignerKey, privateSignerKey);

                expectedSignerPKs[i] = publicSignerKey;

                byte[] roundBytes = BitConverter.GetBytes(round);

                byte[] signerSignature = Ed25519.Sign(roundBytes, expandedSignerKey);

                expectedSignerSignatures[i] = signerSignature;
            }

            using (MemoryStream ms = new MemoryStream(body))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(expectedDateTime.ToBinary());
                    bw.Write(round);
                    bw.Write(signersCount);

                    for (int i = 0; i < signersCount; i++)
                    {
                        bw.Write(expectedSignerPKs[i]);
                        bw.Write(expectedSignerSignatures[i]);
                    }
                }
            }

            byte[] packet = BinaryBuilder.GetSignedPacket(
                PacketType.Synchronization, 
                syncBlockHeight, 
                nonce, powHash, version, 
                BlockTypes.Synchronization_ConfirmedBlock, blockHeight, prevHash, body, privateKey, out expectedSignature);
            string packetExpectedString = packet.ToHexString();

            SynchronizationConfirmedBlockParser synchronizationConfirmedBlockParser = new SynchronizationConfirmedBlockParser(identityKeyProvidersRegistry, hashCalculationRepository);
            SynchronizationConfirmedBlock block = (SynchronizationConfirmedBlock)synchronizationConfirmedBlockParser.Parse(packet);

            string packetActualString = block.RawData.ToHexString();
            Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
            Assert.Equal(nonce, block.Nonce);
            Assert.Equal(powHash, block.PowHash);
            Assert.Equal(version, block.Version);
            Assert.Equal(blockHeight, block.BlockHeight);
            Assert.Equal(prevHash, block.HashPrev);
            Assert.Equal(expectedDateTime, block.ReportedTime);
            Assert.Equal(round, block.Round);
                
            for (int i = 0; i < signersCount; i++)
            {
                Assert.Equal(expectedSignerPKs[i], block.PublicKeys[i]);
                Assert.Equal(expectedSignerSignatures[i], block.Signatures[i]);
            }

            Assert.Equal(publicKey, block.Key.Value);
            Assert.Equal(expectedSignature, block.Signature);
        }

        [Fact]
        public void RegistryRegisterBlockParserTest()
        {
            IIdentityKeyProvider identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();

            identityKeyProvidersRegistry.GetInstance().Returns(identityKeyProvider);
            identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key32() { Value = c.Arg<byte[]>() });

            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = null;

            PacketType expectedReferencedPacketType = PacketType.TransactionalChain;
            ushort expectedReferencedBlockType = BlockTypes.Transaction_TransferFunds;
            ulong expectedReferencedHeight = 123466774;
            byte[] expectedReferencedBodyHash = BinaryBuilder.GetDefaultHash(473826643);
            byte[] expectedTarget = BinaryBuilder.GetDefaultHash(BinaryBuilder.GetRandomPublicKey());

            byte[] body;

            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            byte[] expandedPrivateKey;
            byte[] publicKey;
            Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, privateKey);

            byte[] expectedSignature;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)expectedReferencedPacketType);
                    bw.Write(expectedReferencedBlockType);
                    bw.Write(expectedReferencedHeight);
                    bw.Write(expectedReferencedBodyHash);
                    bw.Write(expectedTarget);
                }

                body = ms.ToArray();
            }

            byte[] packet = BinaryBuilder.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Registry_Register, blockHeight, prevHash, body, privateKey, out expectedSignature);

            RegistryRegisterBlockParser registryRegisterBlockParser = new RegistryRegisterBlockParser(identityKeyProvidersRegistry, hashCalculationRepository);
            RegistryRegisterBlock block = (RegistryRegisterBlock)registryRegisterBlockParser.Parse(packet);

            Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
            Assert.Equal(nonce, block.Nonce);
            Assert.Equal(powHash, block.PowHash);
            Assert.Equal(version, block.Version);
            Assert.Equal(blockHeight, block.BlockHeight);

            Assert.Equal(expectedReferencedPacketType, block.TransactionHeader.ReferencedPacketType);
            Assert.Equal(expectedReferencedBlockType, block.TransactionHeader.ReferencedBlockType);
            Assert.Equal(expectedReferencedHeight, block.TransactionHeader.ReferencedHeight);
            Assert.Equal(expectedReferencedBodyHash, block.TransactionHeader.ReferencedBodyHash);
            Assert.Equal(expectedTarget, block.TransactionHeader.ReferencedTargetHash);

            Assert.Equal(publicKey, block.Key.Value);
            Assert.Equal(expectedSignature, block.Signature);
        }

        [Fact]
        public void RegistryShortBlockParserTest()
        {
            IIdentityKeyProvider transactionHashKeyProvider = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvider identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();

            identityKeyProvidersRegistry.GetInstance().Returns(identityKeyProvider);
            identityKeyProvidersRegistry.GetInstance("TransactionRegistry").Returns(transactionHashKeyProvider);
            identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key32() { Value = c.Arg<byte[]>() });
            transactionHashKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key16() { Value = c.Arg<byte[]>() });

            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = null;

            byte[] body;

            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            byte[] expandedPrivateKey;
            byte[] publicKey;
            Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, privateKey);

            byte[] expectedSignature;
            ushort expectedCount = 10;

            SortedList<ushort, IKey> transactionHeaders = new SortedList<ushort, IKey>();
            for (ushort i = 0; i < expectedCount; i++)
            {
                transactionHeaders.Add(i, new Key16(BinaryBuilder.GetTransactionKeyHash(i)));
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)transactionHeaders.Count);

                    foreach (ushort order in transactionHeaders.Keys)
                    {
                        bw.Write(order);
                        bw.Write(transactionHeaders[order].Value);
                    }
                }

                body = ms.ToArray();
            }

            byte[] packet = BinaryBuilder.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Registry_ShortBlock, blockHeight, prevHash, body, privateKey, out expectedSignature);

            RegistryShortBlockParser registryFullBlockParser = new RegistryShortBlockParser(identityKeyProvidersRegistry, hashCalculationRepository);
            RegistryShortBlock block = (RegistryShortBlock)registryFullBlockParser.Parse(packet);

            Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
            Assert.Equal(nonce, block.Nonce);
            Assert.Equal(powHash, block.PowHash);
            Assert.Equal(version, block.Version);
            Assert.Equal(blockHeight, block.BlockHeight);

            foreach (var item in transactionHeaders)
            {
                Assert.True(block.TransactionHeaderHashes.ContainsKey(item.Key));
                Assert.Equal(item.Value, block.TransactionHeaderHashes[item.Key]);
            }

            Assert.Equal(publicKey, block.Key.Value);
            Assert.Equal(expectedSignature, block.Signature);
        }

        [Fact]
        public void RegistryFullBlockParserTest()
        {
            IIdentityKeyProvider identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();

            identityKeyProvidersRegistry.GetInstance().Returns(identityKeyProvider);
            identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key32() { Value = c.Arg<byte[]>() });

            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = null;

            PacketType expectedReferencedPacketType = PacketType.TransactionalChain;
            ushort expectedReferencedBlockType = BlockTypes.Transaction_TransferFunds;
            ulong expectedReferencedHeight = 123466774;

            byte[] body;

            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            byte[] expandedPrivateKey;
            byte[] publicKey;
            Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, privateKey);

            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            cryptoService.Sign(null).ReturnsForAnyArgs(c => Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey));
            cryptoService.Key.Returns(new Key32() { Value = publicKey });

            byte[] expectedSignature;
            ushort expectedCount = 1000;
            byte[] expectedShortBlockHash;

            SortedList<ushort, RegistryRegisterBlock> transactionHeaders = new SortedList<ushort, RegistryRegisterBlock>();
            for (ushort i = 0; i < expectedCount; i++)
            {
                RegistryRegisterBlock registryRegisterBlock = new RegistryRegisterBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    Nonce = nonce + i,
                    PowHash = BinaryBuilder.GetPowHash(1234 + i),
                    BlockHeight = blockHeight,
                    TransactionHeader = new TransactionHeader
                    {
                        ReferencedPacketType = expectedReferencedPacketType,
                        ReferencedBlockType = expectedReferencedBlockType,
                        ReferencedHeight = expectedReferencedHeight,
                        ReferencedBodyHash = BinaryBuilder.GetDefaultHash(473826643 + i),
                        ReferencedTargetHash = BinaryBuilder.GetDefaultHash(BinaryBuilder.GetRandomPublicKey())
                    }
                };

                RegistryRegisterBlockSerializer serializer1 = new RegistryRegisterBlockSerializer(cryptoService);
                serializer1.Initialize(registryRegisterBlock);
                serializer1.FillBodyAndRowBytes();

                transactionHeaders.Add(i, registryRegisterBlock);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write((ushort)transactionHeaders.Count);

                    foreach (ushort order in transactionHeaders.Keys)
                    {
                        bw.Write(order);
                        bw.Write(transactionHeaders[order].RawData);
                    }

                    expectedShortBlockHash = BinaryBuilder.GetDefaultHash(1111);
                    bw.Write(expectedShortBlockHash);
                }

                body = ms.ToArray();
            }

            byte[] packet = BinaryBuilder.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Registry_FullBlock, blockHeight, prevHash, body, privateKey, out expectedSignature);

            RegistryFullBlockParser registryFullBlockParser = new RegistryFullBlockParser(identityKeyProvidersRegistry, hashCalculationRepository);
            RegistryFullBlock block = (RegistryFullBlock)registryFullBlockParser.Parse(packet);

            Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
            Assert.Equal(nonce, block.Nonce);
            Assert.Equal(powHash, block.PowHash);
            Assert.Equal(version, block.Version);
            Assert.Equal(blockHeight, block.BlockHeight);

            foreach (var item in transactionHeaders)
            {
                Assert.True(block.TransactionHeaders.ContainsKey(item.Key));
                Assert.Equal(item.Value.PacketType, block.TransactionHeaders[item.Key].PacketType);
                Assert.Equal(item.Value.SyncBlockHeight, block.TransactionHeaders[item.Key].SyncBlockHeight);
                Assert.Equal(item.Value.Nonce, block.TransactionHeaders[item.Key].Nonce);
                Assert.Equal(item.Value.PowHash, block.TransactionHeaders[item.Key].PowHash);
                Assert.Equal(item.Value.BlockHeight, block.TransactionHeaders[item.Key].BlockHeight);
                Assert.Equal(item.Value.BlockType, block.TransactionHeaders[item.Key].BlockType);
                Assert.Equal(item.Value.TransactionHeader.ReferencedPacketType, block.TransactionHeaders[item.Key].TransactionHeader.ReferencedPacketType);
                Assert.Equal(item.Value.TransactionHeader.ReferencedBlockType, block.TransactionHeaders[item.Key].TransactionHeader.ReferencedBlockType);
                Assert.Equal(item.Value.TransactionHeader.ReferencedHeight, block.TransactionHeaders[item.Key].TransactionHeader.ReferencedHeight);
                Assert.Equal(item.Value.TransactionHeader.ReferencedBodyHash, block.TransactionHeaders[item.Key].TransactionHeader.ReferencedBodyHash);
                Assert.Equal(item.Value.TransactionHeader.ReferencedTargetHash, block.TransactionHeaders[item.Key].TransactionHeader.ReferencedTargetHash);
                Assert.Equal(item.Value.Signature, block.TransactionHeaders[item.Key].Signature);
                Assert.Equal(item.Value.Key, block.TransactionHeaders[item.Key].Key);
            }

            Assert.Equal(expectedShortBlockHash, block.ShortBlockHash);

            Assert.Equal(publicKey, block.Key.Value);
            Assert.Equal(expectedSignature, block.Signature);
        }

        [Fact]
        public void RegistryConfidenceBlockParserTest()
        {
            IIdentityKeyProvider identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationsRepository hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();

            identityKeyProvidersRegistry.GetInstance().Returns(identityKeyProvider);
            identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key32() { Value = c.Arg<byte[]>() });

            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = null;

            ushort expectedConfidence = 123;
            byte[] expectedReferencedBodyHash = BinaryBuilder.GetDefaultHash(473826643);

            byte[] body;

            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            byte[] expandedPrivateKey;
            byte[] publicKey;
            Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, privateKey);

            byte[] expectedSignature;

            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(expectedConfidence);
                    bw.Write(expectedReferencedBodyHash);
                }

                body = ms.ToArray();
            }

            byte[] packet = BinaryBuilder.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Registry_ConfidenceBlock, blockHeight, prevHash, body, privateKey, out expectedSignature);

            RegistryConfidenceBlockParser registryFullBlockParser = new RegistryConfidenceBlockParser(identityKeyProvidersRegistry, hashCalculationRepository);
            RegistryConfidenceBlock block = (RegistryConfidenceBlock)registryFullBlockParser.Parse(packet);

            Assert.Equal(syncBlockHeight, block.SyncBlockHeight);
            Assert.Equal(nonce, block.Nonce);
            Assert.Equal(powHash, block.PowHash);
            Assert.Equal(version, block.Version);
            Assert.Equal(blockHeight, block.BlockHeight);

            Assert.Equal(expectedConfidence, block.Confidence);
            Assert.Equal(expectedReferencedBodyHash, block.ReferencedBlockHash);

            Assert.Equal(publicKey, block.Key.Value);
            Assert.Equal(expectedSignature, block.Signature);
        }
    }
}
