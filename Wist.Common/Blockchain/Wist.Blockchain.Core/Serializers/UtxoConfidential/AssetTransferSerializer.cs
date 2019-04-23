using System.IO;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.UtxoConfidential
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class AssetTransferSerializer : UtxoConfidentialSerializerBase<AssetTransfer>
    {
        public AssetTransferSerializer() 
            : base(PacketType.UtxoConfidential, BlockTypes.UtxoConfidential_NonQuantitativeAssetTransfer)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.AssetCommitment);
            bw.Write((ushort)_block.SurjectionProof.AssetCommitments.Length);
            for (int i = 0; i < _block.SurjectionProof.AssetCommitments.Length; i++)
            {
                bw.Write(_block.SurjectionProof.AssetCommitments[i]);
            }

            bw.Write(_block.SurjectionProof.Rs.E);

            for (int i = 0; i < _block.SurjectionProof.AssetCommitments.Length; i++)
            {
                bw.Write(_block.SurjectionProof.Rs.S[i]);
            }

            bw.Write(_block.EcdhTuple.Mask);
        }
    }
}
