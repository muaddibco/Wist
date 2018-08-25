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

namespace Wist.BlockLattice.Core.Tests
{
    public class ParsersTests : IClassFixture<DependencyInjectionSupportFixture>
    {
        [Fact]
        public void SynchronizationConfirmedBlockParserTest()
        {
            IIdentityKeyProvider identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            IHashCalculationRepository hashCalculationRepository = Substitute.For<IHashCalculationRepository>();

            identityKeyProvidersRegistry.GetInstance().Returns(identityKeyProvider);
            identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Public32Key() { Value = c.Arg<byte[]>() });

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
            Assert.Equal(powHash, block.HashNonce);
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
    }
}
