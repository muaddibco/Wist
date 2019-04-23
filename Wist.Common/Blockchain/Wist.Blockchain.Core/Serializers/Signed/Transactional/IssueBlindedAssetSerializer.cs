using System.IO;
using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.Signed.Transactional
{
    [RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class IssueBlindedAssetSerializer : TransactionalSerializerBase<IssueBlindedAsset>
	{
		public IssueBlindedAssetSerializer() : base(PacketType.Transactional, BlockTypes.Transaction_IssueBlindedAsset)
		{
		}

		protected override void WriteBody(BinaryWriter bw)
		{
			base.WriteBody(bw);

			bw.Write(_block.GroupId);
			bw.Write(_block.AssetCommitment);
            bw.Write(_block.KeyImage);
			bw.Write(_block.UniquencessProof.C);
			bw.Write(_block.UniquencessProof.R);
		}
	}
}
