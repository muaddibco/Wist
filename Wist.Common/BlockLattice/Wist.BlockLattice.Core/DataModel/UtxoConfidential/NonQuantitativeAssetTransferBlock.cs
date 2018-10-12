using Wist.BlockLattice.Core.Enums;
using Wist.Crypto.ConfidentialAssets;

namespace Wist.BlockLattice.Core.DataModel.UtxoConfidential
{
    public class NonQuantitativeAssetTransferBlock : UtxoConfidentialBase
    {
        public override ushort BlockType => BlockTypes.UtxoConfidential_NonQuantitativeAssetTransfer;

        public override ushort Version => 1;

        public byte[] AssetCommitment { get; set; }

        public AssetRangeProof AssetRangeProof { get; set; }

        public EcdhTupleCA EcdhTuple { get; set; }
    }
}
