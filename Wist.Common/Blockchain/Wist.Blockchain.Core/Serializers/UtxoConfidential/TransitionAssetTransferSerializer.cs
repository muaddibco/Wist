using System.IO;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.Serializers.UtxoConfidential
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransitionAssetTransferSerializer : UtxoConfidentialSerializerBase<TransitionAffiliatedAssetTransfer>
    {
        public TransitionAssetTransferSerializer()
            : base(PacketType.UtxoConfidential, BlockTypes.UtxoConfidential_NonQuantitativeTransitionAssetTransfer)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            WriteCommitmentAndProof(bw, _block.AssetCommitment, _block.SurjectionProof);
            WriteCommitmentAndProof(bw, _block.AffiliationCommitment, _block.AffiliationSurjectionProof);

            bw.Write((ushort)_block.AffiliationKeys.Length);
            for (int i = 0; i < _block.AffiliationKeys.Length; i++)
            {
                bw.Write(_block.AffiliationKeys[i]);
            }

            WriteBorromeanRingSignature(bw, _block.AffiliationBorromeanSignature);

            WriteCommitmentAndProof(bw, null, _block.AffiliationEvidenceSurjectionProof);

            WriteEcdhTupleCA(bw, _block.EcdhTuple);
        }

        private void WriteCommitmentAndProof(BinaryWriter bw, byte[] commitment, SurjectionProof surjectionProof)
        {
            if (commitment != null)
            {
                bw.Write(commitment);
            }

            WriteSurjectionProof(bw, surjectionProof);
        }
    }
}
