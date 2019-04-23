using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.UtxoConfidential
{
	public class TransitionAuthenticationProofs : UtxoConfidentialBase
	{
		public override ushort Version => 1;

		public override ushort BlockType => BlockTypes.UtxoConfidential_TransitionAuthenticationProofs;

		/// <summary>
		/// C = x * G + I, where I is elliptic curve point representing assert id
		/// </summary>
		public byte[] AssetCommitment { get; set; }

		public EcdhTupleProofs EncodedPayload { get; set; }

		/// <summary>
		/// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
		/// </summary>
		public SurjectionProof OwnershipProof { get; set; }

		/// <summary>
		/// Contain Surjection Proofs to assets issued by Asset Issuer specified at EcdhTuple for checking eligibility purposes
		/// </summary>
		public SurjectionProof EligibilityProof { get; set; }

		/// <summary>
		/// Contain Surjection Proofs to the commitment of registration, where value of commitment is encoded using shared secret (one time secret key * Target Public Key)
		/// </summary>
		public SurjectionProof AuthenticationProof { get; set; }
	}
}
