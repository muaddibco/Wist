using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class IssueAssociatedBlindedAssetSerializer : TransactionalSerializerBase<IssueAssociatedBlindedAsset>
	{
		public IssueAssociatedBlindedAssetSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_IssueAssociatedBlindedAsset)
		{
		}

		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			bw.Write(_block.GroupId);
			bw.Write(_block.AssetCommitment);
            bw.Write(_block.RootAssetCommitment);
		}
	}
}
