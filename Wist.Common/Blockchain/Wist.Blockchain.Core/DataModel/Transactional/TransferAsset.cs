using Wist.Blockchain.Core.DataModel.Transactional.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.Transactional
{
	public class TransferAsset : TransactionalTransitionalPacketBase
	{
		public override ushort Version => 1;

		public override ushort BlockType => BlockTypes.Transaction_TransferAsset;

		public EncryptedAsset TransferredAsset { get; set; }

		/// <summary>
		/// Surjection Proof contains reference to AssetCommitment that is being transferred. If referenced AssetCommitment is TransferredAsset of already sent commitment so it cancels previous sending
		/// </summary>
		public SurjectionProof SurjectionProof { get; set; }
	}
}
