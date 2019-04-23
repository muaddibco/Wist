using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Core;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Parsers.UtxoConfidential
{
    public abstract class UtxoConfidentialParserBase : BlockParserBase
    {
        public UtxoConfidentialParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override PacketType PacketType => PacketType.UtxoConfidential;

        protected override PacketBase ParseBlockBase(ushort version, Memory<byte> spanBody, out Memory<byte> spanPostBody)
        {
            int readBytes = 0;

            byte[] destinationKey = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			byte[] destinationKey2 = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
			readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			byte[] transactionPublicKey = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            spanPostBody = ParseUtxoConfidential(version, spanBody.Slice(readBytes), out UtxoConfidentialBase utxoConfidentialBase);

            ushort readBytesPostBody = 0;
            Memory<byte> keyImage = spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE);
            readBytesPostBody += Globals.NODE_PUBLIC_KEY_SIZE;

            ushort ringSignaturesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanPostBody.Span.Slice(readBytesPostBody));
            readBytesPostBody += sizeof(ushort);

            utxoConfidentialBase.KeyImage = _entityIdentityKeyProvider.GetKey(keyImage);
            utxoConfidentialBase.DestinationKey = destinationKey;
			utxoConfidentialBase.DestinationKey2 = destinationKey2;
            utxoConfidentialBase.TransactionPublicKey = transactionPublicKey;
            utxoConfidentialBase.PublicKeys = new IKey[ringSignaturesCount];
            utxoConfidentialBase.Signatures = new RingSignature[ringSignaturesCount];

            for (int i = 0; i < ringSignaturesCount; i++)
            {
                byte[] publicKey = spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                IKey key = _entityIdentityKeyProvider.GetKey(spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE));
                utxoConfidentialBase.PublicKeys[i] = key;
                readBytesPostBody += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            for (int i = 0; i < ringSignaturesCount; i++)
            {
                byte[] c = spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytesPostBody += Globals.NODE_PUBLIC_KEY_SIZE;

                byte[] r = spanPostBody.Slice(readBytesPostBody, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytesPostBody += Globals.NODE_PUBLIC_KEY_SIZE;

                RingSignature ringSignature = new RingSignature { C = c, R = r };
                utxoConfidentialBase.Signatures[i] = ringSignature;
            }

            spanPostBody = spanPostBody.Slice(readBytesPostBody);

            return utxoConfidentialBase;
        }

        protected abstract Memory<byte> ParseUtxoConfidential(ushort version, Memory<byte> spanBody, out UtxoConfidentialBase utxoConfidentialBase);

        protected static void ReadCommitment(ref Memory<byte> spanBody, ref int readBytes, out byte[] assetCommitment)
        {
            assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
        }

        protected static void ReadSurjectionProof(ref Memory<byte> spanBody, ref int readBytes, out SurjectionProof surjectionProof)
        {
            ushort assetCommitmentsCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Slice(readBytes).Span);
            readBytes += sizeof(ushort);

            byte[][] assetCommitments = new byte[assetCommitmentsCount][];
            for (int i = 0; i < assetCommitmentsCount; i++)
            {
                assetCommitments[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            byte[] e = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[][] s = new byte[assetCommitmentsCount][];
            for (int i = 0; i < assetCommitmentsCount; i++)
            {
                s[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            surjectionProof = new SurjectionProof
            {
                AssetCommitments = assetCommitments,
                Rs = new BorromeanRingSignature
                {
                    E = e,
                    S = s
                }
            };
        }

        protected static void ReadBorromeanRingSignature(ref Memory<byte> spanBody, ref int readBytes, out BorromeanRingSignature borromeanRingSignature)
        {
            byte[] e = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ushort sCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
            readBytes += 2;

            byte[][] s = new byte[sCount][];
            for (int i = 0; i < sCount; i++)
            {
                s[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
            }

            borromeanRingSignature = new BorromeanRingSignature { E = e, S = s };
        }

        protected void ReadEcdhTupleCA(ref Memory<byte> spanBody, ref int readBytes, out EcdhTupleCA ecdhTuple)
        {
            byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ecdhTuple = new EcdhTupleCA
            {
                Mask = mask,
                AssetId = assetId
            };
        }

		protected void ReadEcdhTupleIP(ref Memory<byte> spanBody, ref int readBytes, out EcdhTupleIP ecdhTuple)
		{
			byte[] issuer = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
			readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			byte[] payload = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
			readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

			ecdhTuple = new EcdhTupleIP
			{
				Issuer = issuer,
				Payload = payload
			};
		}

		protected void ReadEcdhTupleProofs(ref Memory<byte> spanBody, ref int readBytes, out EcdhTupleProofs ecdhTuple)
        {
            byte[] mask = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] assetId = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] assetIssuer = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            byte[] payload = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ecdhTuple = new EcdhTupleProofs
            {
                Mask = mask,
                AssetId = assetId,
                AssetIssuer = assetIssuer,
                Payload = payload
            };
        }
    }
}
