using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransferGroupedAssetsSerializer : TransactionalSerializerBase<TransferGroupedAssets>
    {
        public TransferGroupedAssetsSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_TransferGroupedAssets)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.Target);

            bw.Write((ushort)(_block.TransferedAssets?.Length??0));

            for (int i = 0; i < _block.TransferedAssets.Length; i++)
            {
                bw.Write(_block.TransferedAssets[i].AssetCommitment);
                bw.Write(_block.TransferedAssets[i].EcdhTuple.AssetId);
                bw.Write(_block.TransferedAssets[i].EcdhTuple.Mask);
            }

            bw.Write((ushort)_block.BlindedAssetsGroups.Length);
            for (int i = 0; i < _block.BlindedAssetsGroups.Length; i++)
            {
                bw.Write(_block.BlindedAssetsGroups[i].GroupId);
                bw.Write((ushort)_block.BlindedAssetsGroups[i].AssetCommitments.Length);

                for (int j = 0; j < _block.BlindedAssetsGroups[i].AssetCommitments.Length; j++)
                {
                    bw.Write(_block.BlindedAssetsGroups[i].AssetCommitments[j]);
                }
            }

            for (int i = 0; i < _block.InversedSurjectionProofs.Length; i++)
            {
                WriteInversedSurjectionProof(bw, _block.InversedSurjectionProofs[i]);
            }
        }
    }
}
