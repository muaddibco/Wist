using Wist.BlockLattice.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.BlockLattice.Core.DataModel.UtxoConfidential
{
    public class NonQuantitativeAssetTransferBlock : UtxoConfidentialContentBase
    {
        public override ushort BlockType => BlockTypes.UtxoConfidential_NonQuantitativeAssetTransfer;

        public override ushort Version => 1;

        public byte[] AssetCommitment { get; set; }

        public SurjectionProof AssetRangeProof { get; set; }

        public EcdhTupleCA EcdhTuple { get; set; }
    }
}
