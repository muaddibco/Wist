using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.UtxoConfidential
{
    public class TransitionAffiliatedAssetTransfer : UtxoConfidentialBase
    {
        public override ushort BlockType => BlockTypes.UtxoConfidential_NonQuantitativeTransitionAssetTransfer;

        public override ushort Version => 1;

        /// <summary>
        /// C = x * G + I, where I is elliptic curve point representing assert id
        /// </summary>
        public byte[] AssetCommitment { get; set; }

        public SurjectionProof SurjectionProof { get; set; }

        public byte[] AffiliationCommitment { get; set; }

        public SurjectionProof AffiliationSurjectionProof { get; set; }

        public byte[][] AffiliationKeys { get; set; }

        public BorromeanRingSignature AffiliationBorromeanSignature { get; set; }

        public SurjectionProof AffiliationEvidenceSurjectionProof { get; set; }

        /// <summary>
        /// Contains encrypted blinding factor of AssetCommitment: x` = x ^ (r * A). To decrypt receiver makes (R * a) ^ x` = x.
        /// </summary>
        public EcdhTupleCA EcdhTuple { get; set; }
    }
}
