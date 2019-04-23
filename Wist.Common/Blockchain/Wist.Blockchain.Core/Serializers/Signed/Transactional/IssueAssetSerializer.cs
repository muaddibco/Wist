using System.IO;
using System.Text;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class IssueAssetSerializer : TransactionalSerializerBase<IssueAsset>
    {
        public IssueAssetSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_IssueAsset)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            base.WriteBody(bw);

            bw.Write(_block.AssetIssuance.AssetId);
            byte strLen = (byte)_block.AssetIssuance.IssuedAssetInfo.Length;
            bw.Write(strLen);
            bw.Write(Encoding.ASCII.GetBytes(_block.AssetIssuance.IssuedAssetInfo.Substring(0, strLen)));
        }
    }
}
