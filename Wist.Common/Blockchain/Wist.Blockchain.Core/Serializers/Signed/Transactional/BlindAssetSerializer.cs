using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class BlindAssetSerializer : TransactionalSerializerBase<BlindAsset>
    {
        public BlindAssetSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_BlindAsset)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.EncryptedAsset.AssetCommitment);
            bw.Write(_block.EncryptedAsset.EcdhTuple.Mask);
            bw.Write(_block.EncryptedAsset.EcdhTuple.AssetId);
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
