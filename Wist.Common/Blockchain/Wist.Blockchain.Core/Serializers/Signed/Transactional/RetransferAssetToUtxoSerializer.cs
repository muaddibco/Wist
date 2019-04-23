using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
	public class RetransferAssetToUtxoSerializer : TransactionalTransitionalSerializerBase<TransferAssetToUtxo>
    {
        public RetransferAssetToUtxoSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_RetransferAssetToUtxo)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.TransferredAsset.AssetCommitment);
            bw.Write(_block.TransferredAsset.EcdhTuple.Mask);
            bw.Write(_block.TransferredAsset.EcdhTuple.AssetId);
            bw.Write((ushort)_block.SurjectionProof.AssetCommitments.Length);

            for (int i = 0; i < _block.SurjectionProof.AssetCommitments.Length; i++)
            {
                bw.Write(_block.SurjectionProof.AssetCommitments[i]);
            }

            bw.Write(_block.SurjectionProof.Rs.E);

            for (int i = 0; i < _block.SurjectionProof.Rs.S.Length; i++)
            {
                bw.Write(_block.SurjectionProof.Rs.S[i]);
            }
        }
    }
}
