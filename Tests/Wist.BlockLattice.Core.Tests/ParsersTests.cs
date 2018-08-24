using Chaos.NaCl;
using HashLib;
using System;
using System.IO;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Parsers.Synchronization;
using Wist.Tests.Core;
using Wist.Tests.Core.Fixtures;
using Xunit;

namespace Wist.BlockLattice.Core.Tests
{
    public class ParsersTests : IClassFixture<DependencyInjectionSupportFixture>
    {
        [Fact]
        public void SynchronizationConfirmedBlockParserTest()
        {
            byte[] privateKey = BinaryBuilder.GetRandomSeed();
            byte signersCount = 10;
            byte[] body = new byte[3 + Globals.NODE_PUBLIC_KEY_SIZE * signersCount + Globals.SIGNATURE_SIZE * signersCount];
            ushort round = 1;

            using (MemoryStream ms = new MemoryStream(body))
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(round);
                    bw.Write(signersCount);

                    for (int i = 0; i < signersCount; i++)
                    {
                        byte[] privateSignerKey = BinaryBuilder.GetRandomSeed();
                        byte[] publicSignerKey;
                        byte[] expandedSignerKey;

                        Ed25519.KeyPairFromSeed(out publicSignerKey, out expandedSignerKey, privateSignerKey);

                        byte[] roundBytes = BitConverter.GetBytes(round);

                        byte[] signerSignature = Ed25519.Sign(roundBytes, expandedSignerKey);

                        bw.Write(publicSignerKey);
                        bw.Write(signerSignature);
                    }
                }
            }

            byte[] powHash = BinaryBuilder.GetPowHash(1234);
            byte[] prevHash = BinaryBuilder.GetPrevHash(1234);

            byte[] packet = BinaryBuilder.GetSignedPacket(PacketType.Synchronization, 1, 2, powHash, 1, BlockTypes.Synchronization_ConfirmedBlock, 9, prevHash, body, privateKey);

            
        }
    }
}
