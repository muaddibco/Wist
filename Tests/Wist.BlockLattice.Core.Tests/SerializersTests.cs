using Chaos.NaCl;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Serializers.Signed.Synchronization;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Tests.Core;
using Wist.Tests.Core.Fixtures;
using Wist.Core.ExtensionMethods;
using Xunit;
using System.IO;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Serializers.Signed.Registry;
using System.Diagnostics;

namespace Wist.BlockLattice.Core.Tests
{
    public class SerializersTests : IClassFixture<DependencyInjectionSupportFixture>
    {
        [Fact]
        public void SynchronizationConfirmedBlockSerializerTest()
        {
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] prevHash = BinaryBuilder.GetDefaultHash(1234);

            ushort round = 1;
            byte signersCount = 10;
            byte[] body = new byte[11 + Globals.NODE_PUBLIC_KEY_SIZE * signersCount + Globals.SIGNATURE_SIZE * signersCount];

            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            byte[] expandedPrivateKey;
            byte[] publicKey;
            Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, privateKey);

            byte[] expectedSignature;
            byte[][] expectedSignerPKs = new byte[signersCount][];
            byte[][] expectedSignerSignatures = new byte[signersCount][];

            DateTime expectedDateTime = DateTime.Now;


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

            byte[] expectedPacket = BinaryBuilder.GetSignedPacket(
                PacketType.Synchronization,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Synchronization_ConfirmedBlock, blockHeight, prevHash, body, privateKey, out expectedSignature);

            SynchronizationConfirmedBlock block = new SynchronizationConfirmedBlock()
            {
                SyncBlockHeight = syncBlockHeight,
                BlockHeight = blockHeight,
                Nonce = nonce,
                PowHash = powHash,
                HashPrev = prevHash,
                Key = new Key32() { Value = publicKey },
                ReportedTime = expectedDateTime,
                Round = round,
                Signature = expectedSignature,
                PublicKeys = new byte[signersCount][],
                Signatures = new byte[signersCount][]
            };

            for (int i = 0; i < signersCount; i++)
            {
                block.PublicKeys[i] = expectedSignerPKs[i];
                block.Signatures[i] = expectedSignerSignatures[i];
            }

            block.Signature = expectedSignature;

            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            cryptoService.Sign(null).ReturnsForAnyArgs(c => Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey));
            cryptoService.Key.Returns(new Key32() { Value = publicKey });

            SynchronizationConfirmedBlockSerializer serializer = new SynchronizationConfirmedBlockSerializer(cryptoService);
            serializer.Initialize(block);

            byte[] actualPacket = serializer.GetBytes();

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void RegistryRegisterBlockSerializerTest()
        {
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

            byte[] expectedPacket = BinaryBuilder.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Registry_Register, blockHeight, prevHash, body, privateKey, out expectedSignature);

            RegistryRegisterBlock block = new RegistryRegisterBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                TransactionHeader = new TransactionHeader
                {
                    ReferencedPacketType = expectedReferencedPacketType,
                    ReferencedBlockType = expectedReferencedBlockType,
                    ReferencedHeight = expectedReferencedHeight,
                    ReferencedBodyHash = expectedReferencedBodyHash,
                    ReferencedTargetHash = expectedTarget
                }
            };

            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            cryptoService.Sign(null).ReturnsForAnyArgs(c => Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey));
            cryptoService.Key.Returns(new Key32() { Value = publicKey });

            RegistryRegisterBlockSerializer serializer = new RegistryRegisterBlockSerializer(cryptoService);
            serializer.Initialize(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void RegistryShortBlockSerializerTest()
        {
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

            byte[] expectedPacket = BinaryBuilder.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Registry_ShortBlock, blockHeight, prevHash, body, privateKey, out expectedSignature);

            RegistryShortBlock block = new RegistryShortBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                TransactionHeaderHashes = transactionHeaders
            };

            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            cryptoService.Sign(null).ReturnsForAnyArgs(c => Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey));
            cryptoService.Key.Returns(new Key32() { Value = publicKey });

            RegistryShortBlockSerializer serializer = new RegistryShortBlockSerializer(cryptoService);
            serializer.Initialize(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void RegistryFullBlockSerializerTest()
        {
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

            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            cryptoService.Sign(null).ReturnsForAnyArgs(c => Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey));
            cryptoService.Key.Returns(new Key32() { Value = publicKey });

            byte[] expectedSignature;
            ushort expectedCount = 10;

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
                }

                body = ms.ToArray();
            }

            byte[] expectedPacket = BinaryBuilder.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Registry_FullBlock, blockHeight, prevHash, body, privateKey, out expectedSignature);

            RegistryFullBlock block = new RegistryFullBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                TransactionHeaders = transactionHeaders
            };

            RegistryFullBlockSerializer serializer = new RegistryFullBlockSerializer(cryptoService);
            serializer.Initialize(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }

        [Fact]
        public void RegistryConfidenceBlockSerializerTest()
        {
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

            byte[] expectedPacket = BinaryBuilder.GetSignedPacket(
                PacketType.Registry,
                syncBlockHeight,
                nonce, powHash, version,
                BlockTypes.Registry_ConfidenceBlock, blockHeight, prevHash, body, privateKey, out expectedSignature);

            RegistryConfidenceBlock block = new RegistryConfidenceBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Nonce = nonce,
                PowHash = powHash,
                BlockHeight = blockHeight,
                Confidence = expectedConfidence,
                ReferencedBlockHash = expectedReferencedBodyHash
            };

            ICryptoService cryptoService = Substitute.For<ICryptoService>();
            cryptoService.Sign(null).ReturnsForAnyArgs(c => Ed25519.Sign(c.Arg<byte[]>(), expandedPrivateKey));
            cryptoService.Key.Returns(new Key32() { Value = publicKey });

            RegistryConfidenceBlockSerializer serializer = new RegistryConfidenceBlockSerializer(cryptoService);
            serializer.Initialize(block);

            byte[] actualPacket = serializer.GetBytes();

            Trace.WriteLine(expectedPacket.ToHexString());
            Trace.WriteLine(actualPacket.ToHexString());

            Assert.Equal(expectedPacket, actualPacket);
        }
    }
}
