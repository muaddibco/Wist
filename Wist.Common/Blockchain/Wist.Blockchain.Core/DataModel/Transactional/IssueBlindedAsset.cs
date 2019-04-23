using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
    public class IssueBlindedAsset : TransactionalPacketBase
	{
		public override ushort Version => 1;

		public override ushort BlockType => BlockTypes.Transaction_IssueBlindedAsset;

        public byte[] GroupId { get; set; }

        public byte[] AssetCommitment { get; set; }

		public byte[] KeyImage { get; set; }

		public RingSignature UniquencessProof { get; set; }
	}
}
