using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.UtxoConfidential;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Parsers.UtxoConfidential
{
    public abstract class UtxoConfidentialParserBase : BlockParserBase
    {
        public UtxoConfidentialParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override PacketType PacketType => PacketType.UtxoConfidential;

        protected override BlockBase ParseBlockBase(ushort version, Memory<byte> spanBody, out Memory<byte> spanPostBody)
        {
            byte[] keyImage = spanBody.Slice(0, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            byte[] destinationKey = spanBody.Slice(Globals.NODE_PUBLIC_KEY_SIZE, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();

            spanPostBody = ParseUtxoConfidential(version, spanBody.Slice(Globals.NODE_PUBLIC_KEY_SIZE + Globals.NODE_PUBLIC_KEY_SIZE), out UtxoConfidentialBase utxoConfidentialBase);

            ushort ringSignaturesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanPostBody.Span);

            utxoConfidentialBase.PublicKeys = new IKey[ringSignaturesCount];
            utxoConfidentialBase.Signatures = new RingSignature[ringSignaturesCount];

            int readBytes = 2;

            for (int i = 0; i < ringSignaturesCount; i++)
            {
                byte[] publicKey = spanPostBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                IKey key = _entityIdentityKeyProvider.GetKey(spanPostBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE));
                utxoConfidentialBase.PublicKeys[i] = key;
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            for (int i = 0; i < ringSignaturesCount; i++)
            {
                byte[] c = spanPostBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] r = spanPostBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

                RingSignature ringSignature = new RingSignature { C = c, R = r };
                utxoConfidentialBase.Signatures[i] = ringSignature;
            }

            return utxoConfidentialBase;
        }

        protected override Memory<byte> SliceInitialBytes(Memory<byte> span, out Memory<byte> spanHeader)
        {
            Memory<byte> span1 = base.SliceInitialBytes(span, out spanHeader);

            spanHeader = span.Slice(0, spanHeader.Length + 8 + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);

            return span1.Slice(8 + Globals.NONCE_LENGTH + Globals.POW_HASH_SIZE);
        }

        protected abstract Memory<byte> ParseUtxoConfidential(ushort version, Memory<byte> spanBody, out UtxoConfidentialBase utxoConfidentialBase);
    }
}
