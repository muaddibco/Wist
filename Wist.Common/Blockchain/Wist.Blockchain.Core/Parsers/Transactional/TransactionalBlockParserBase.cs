using System;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;
using System.Buffers.Binary;
using Wist.Core.Cryptography;
using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Core.Models;
using Wist.Core;

namespace Wist.Blockchain.Core.Parsers.Transactional
{
    public abstract class TransactionalBlockParserBase : SignedBlockParserBase
    {
        public TransactionalBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override PacketType PacketType => PacketType.Transactional;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            int readBytes = 0;
            ulong funds = BinaryPrimitives.ReadUInt64LittleEndian(spanBody.Span.Slice(readBytes));
            readBytes += sizeof(ulong);

            Memory<byte> spanPostBody = ParseTransactional(version, spanBody.Slice(readBytes), out TransactionalPacketBase transactionalBlockBase);
            transactionalBlockBase.UptodateFunds = funds;
            syncedBlockBase = transactionalBlockBase;

            return spanPostBody;
        }

        protected static SurjectionProof ReadSurjectionProof(Span<byte> span, out int readBytes)
        {

            ushort assetCommitmentsLength = BinaryPrimitives.ReadUInt16LittleEndian(span);
            readBytes = sizeof(ushort);

            byte[][] assetCommitments = new byte[assetCommitmentsLength][];

            for (int i = 0; i < assetCommitmentsLength; i++)
            {
                assetCommitments[i] = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            byte[] e = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[][] s = new byte[assetCommitmentsLength][];

            for (int i = 0; i < assetCommitmentsLength; i++)
            {
                s[i] = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            BorromeanRingSignature rs = new BorromeanRingSignature
            {
                E = e,
                S = s
            };

            SurjectionProof surjectionProof = new SurjectionProof
            {
                AssetCommitments = assetCommitments,
                Rs = rs
            };

            return surjectionProof;
        }


        protected static InversedSurjectionProof ReadInversedSurjectionProof(Span<byte> span, out int readBytes)
        {
            readBytes = 0;
            byte[] assetCommitment = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] e = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ushort assetCommitmentsLength = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(readBytes));
            readBytes += sizeof(ushort);

            byte[][] s = new byte[assetCommitmentsLength][];

            for (int i = 0; i < assetCommitmentsLength; i++)
            {
                s[i] = span.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            BorromeanRingSignature rs = new BorromeanRingSignature
            {
                E = e,
                S = s
            };

            InversedSurjectionProof surjectionProof = new InversedSurjectionProof
            {
                AssetCommitment = assetCommitment,
                Rs = rs
            };

            return surjectionProof;
        }

        protected abstract Memory<byte> ParseTransactional(ushort version, Memory<byte> spanBody, out TransactionalPacketBase transactionalBlockBase);
    }
}
