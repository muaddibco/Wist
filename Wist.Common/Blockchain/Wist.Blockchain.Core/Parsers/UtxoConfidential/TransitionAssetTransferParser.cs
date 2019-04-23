using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.UtxoConfidential
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class TransitionAssetTransferParser : UtxoConfidentialParserBase
    {
        public TransitionAssetTransferParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.UtxoConfidential_NonQuantitativeTransitionAssetTransfer;

        protected override Memory<byte> ParseUtxoConfidential(ushort version, Memory<byte> spanBody, out UtxoConfidentialBase utxoConfidentialBase)
        {
            UtxoConfidentialBase block = null;

            if (version == 1)
            {
                int readBytes = 0;

                ReadCommitmentAndProof(ref spanBody, ref readBytes, out byte[] assetCommitment, out SurjectionProof surjectionProof);
                ReadCommitmentAndProof(ref spanBody, ref readBytes, out byte[] affiliationAssetCommitment, out SurjectionProof affiliationSurjectionProof);

                ushort affiliationKeysCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
                readBytes += sizeof(ushort);

                byte[][] affiliationKeys = new byte[affiliationKeysCount][];
                for (int i = 0; i < affiliationKeysCount; i++)
                {
                    affiliationKeys[i] = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                    readBytes += Globals.NODE_PUBLIC_KEY_SIZE;
                }

                ReadBorromeanRingSignature(ref spanBody, ref readBytes, out BorromeanRingSignature borromeanRingSignature);

                ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof surjectionEvidenceProof);

                ReadEcdhTupleCA(ref spanBody, ref readBytes, out EcdhTupleCA ecdhTuple);

                block = new TransitionAffiliatedAssetTransfer
                {
                    AssetCommitment = assetCommitment,
                    SurjectionProof = surjectionProof,
                    AffiliationCommitment = affiliationAssetCommitment,
                    AffiliationSurjectionProof = affiliationSurjectionProof,
                    AffiliationKeys = affiliationKeys,
                    AffiliationBorromeanSignature = borromeanRingSignature,
                    AffiliationEvidenceSurjectionProof = surjectionEvidenceProof,
                    EcdhTuple = ecdhTuple
                };

                utxoConfidentialBase = block;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }

        private static void ReadCommitmentAndProof(ref Memory<byte> spanBody, ref int readBytes, out byte[] assetCommitment, out SurjectionProof surjectionProof)
        {
            assetCommitment = spanBody.Slice(readBytes, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
            readBytes += Globals.NODE_PUBLIC_KEY_SIZE;

            ReadSurjectionProof(ref spanBody, ref readBytes, out surjectionProof);
        }
    }
}
