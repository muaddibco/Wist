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

namespace Wist.BlockLattice.Core.Tests
{
    public class SerializersTests : IClassFixture<DependencyInjectionSupportFixture>
    {
        [Fact]
        public void SynchronizationConfirmedBlockSerializerTest()
        {
            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            byte signersCount = 10;
            byte[] body = new byte[11 + Globals.NODE_PUBLIC_KEY_SIZE * signersCount + Globals.SIGNATURE_SIZE * signersCount];
            ushort round = 1;
            ulong syncBlockHeight = 1;
            uint nonce = 4;
            ushort version = 1;
            ulong blockHeight = 9;
            byte[] expandedPrivateKey;
            byte[] publicKey;
            Ed25519.KeyPairFromSeed(out publicKey, out expandedPrivateKey, privateKey);
            byte[] expectedSignature;

            byte[][] expectedSignerPKs = new byte[signersCount][];
            byte[][] expectedSignerSignatures = new byte[signersCount][];

            DateTime expectedDateTime = DateTime.Now;

            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            byte[] prevHash = BinaryBuilder.GetPrevHash(1234);

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
                HashNonce = powHash,
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

            SynchronizationConfirmedBlockSerializer serializer = new SynchronizationConfirmedBlockSerializer(cryptoService);
            serializer.Initialize(block);

            byte[] actualPacket = serializer.GetBytes();

            Assert.Equal(expectedPacket, actualPacket);
        }
    }
}
