using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class AcceptAssetsSerializer : TransactionalSerializerBase<AcceptAssets>
    {
        public AcceptAssetsSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_AcceptAssets)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write((ushort)(_block.AcceptedAssetUnblindingABs?.Length??0));
            bw.Write((ushort)(_block.AcceptedAssetUnblindingUtxos?.Length??0));

            for (int i = 0; i < (_block.AcceptedAssetUnblindingABs?.Length??0); i++)
            {
                bw.Write(_block.AcceptedAssetUnblindingABs[i].AcceptedAssetsUnblinding.AssetId);
                bw.Write(_block.AcceptedAssetUnblindingABs[i].AcceptedAssetsUnblinding.BlindingFactor);
                bw.Write(_block.AcceptedAssetUnblindingABs[i].SourceAddress);
                bw.Write(_block.AcceptedAssetUnblindingABs[i].SourceHeight);
            }

            for (int i = 0; i < (_block.AcceptedAssetUnblindingUtxos?.Length ?? 0); i++)
            {
                bw.Write(_block.AcceptedAssetUnblindingUtxos[i].AcceptedAssetsUnblinding.AssetId);
                bw.Write(_block.AcceptedAssetUnblindingUtxos[i].AcceptedAssetsUnblinding.BlindingFactor);
                bw.Write(_block.AcceptedAssetUnblindingUtxos[i].SourceKey);
            }

            bw.Write((ushort)(_block.AssetsGroups?.Length??0));

            for (int i = 0; i < (_block.AssetsGroups?.Length>>0); i++)
            {
                bw.Write(_block.AssetsGroups[i].GroupId);

                bw.Write((ushort)_block.AssetsGroups[i].AssetIds.Length);

                for (int j = 0; j < _block.AssetsGroups[i].AssetIds.Length; j++)
                {
                    bw.Write(_block.AssetsGroups[i].AssetIds[j]);
                }

                for (int j = 0; j < _block.AssetsGroups[i].AssetIds.Length; j++)
                {
                    bw.Write(_block.AssetsGroups[i].AssetAmounts[j]);
                }
            }
        }
    }
}
