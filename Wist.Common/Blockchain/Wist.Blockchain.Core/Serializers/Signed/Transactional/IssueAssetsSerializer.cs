using System.IO;
using System.Text;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class IssueAssetsSerializer : TransactionalSerializerBase<IssueGroupedAssets>
    {
        public IssueAssetsSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_IssueGroupedAssets)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write((ushort)(_block.AssetsIssuanceGroups?.Length??0));
            bw.Write((ushort)(_block.BlindedAssetsIssuanceGroups?.Length??0));

            for (int i = 0; i < (_block.AssetsIssuanceGroups?.Length??0); i++)
            {
                bw.Write(_block.AssetsIssuanceGroups[i].GroupId);
                bw.Write((ushort)_block.AssetsIssuanceGroups[i].AssetIssuances.Length);

                for (int j = 0; j < _block.AssetsIssuanceGroups[i].AssetIssuances.Length; j++)
                {
                    bw.Write(_block.AssetsIssuanceGroups[i].AssetIssuances[j].AssetId);
                    byte strLen = (byte)_block.AssetsIssuanceGroups[i].AssetIssuances[j].IssuedAssetInfo.Length;
                    bw.Write(strLen);
                    bw.Write(Encoding.ASCII.GetBytes(_block.AssetsIssuanceGroups[i].AssetIssuances[j].IssuedAssetInfo.Substring(0, strLen)));
                }
            }

            for (int i = 0; i < (_block.BlindedAssetsIssuanceGroups?.Length??0); i++)
            {
                bw.Write(_block.BlindedAssetsIssuanceGroups[i].GroupId);
                bw.Write((ushort)_block.BlindedAssetsIssuanceGroups[i].AssetCommitments.Length);

                for (int j = 0; j < _block.BlindedAssetsIssuanceGroups[i].AssetCommitments.Length; j++)
                {
                    bw.Write(_block.BlindedAssetsIssuanceGroups[i].AssetCommitments[j]);
                }

                for (int j = 0; j < _block.BlindedAssetsIssuanceGroups[i].AssetIssuances.Length; j++)
                {
                    bw.Write(_block.BlindedAssetsIssuanceGroups[i].AssetIssuances[j].AssetId);
                    byte strLen = (byte)_block.BlindedAssetsIssuanceGroups[i].AssetIssuances[j].IssuedAssetInfo.Length;
                    bw.Write(strLen);
                    bw.Write(Encoding.ASCII.GetBytes(_block.BlindedAssetsIssuanceGroups[i].AssetIssuances[j].IssuedAssetInfo.Substring(0, strLen)));
                }

                for (int j = 0; j < _block.BlindedAssetsIssuanceGroups[i].AssetIssuances.Length; j++)
                {
                    WriteSurjectionProof(bw, _block.BlindedAssetsIssuanceGroups[i].IssuanceProofs[j].SurjectionProof);
                    bw.Write(_block.BlindedAssetsIssuanceGroups[i].IssuanceProofs[j].Mask);
                }
            }

            byte strLen2 = (byte)_block.IssuanceInfo.Length;
            bw.Write(strLen2);
            bw.Write(Encoding.ASCII.GetBytes(_block.IssuanceInfo.Substring(0, strLen2)));

            bw.Write((ushort)(_block.AssetsGroups?.Length ?? 0));
            bw.Write((ushort)(_block.BlindedAssetsGroups?.Length ?? 0));

            for (int i = 0; i < (_block.AssetsGroups?.Length??0); i++)
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

            for (int i = 0; i < (_block.BlindedAssetsGroups?.Length??0); i++)
            {
                bw.Write(_block.BlindedAssetsGroups[i].GroupId);
                bw.Write((ushort)_block.BlindedAssetsGroups[i].AssetCommitments.Length);

                for (int j = 0; j < _block.BlindedAssetsGroups[i].AssetCommitments.Length; j++)
                {
                    bw.Write(_block.BlindedAssetsGroups[i].AssetCommitments[j]);
                }
            }
        }
    }
}
