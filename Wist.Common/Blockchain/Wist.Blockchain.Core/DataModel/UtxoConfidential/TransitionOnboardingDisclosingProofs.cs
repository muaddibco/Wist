using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.DataModel.UtxoConfidential.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.UtxoConfidential
{
    /// <summary>
    /// This transaction does not transfer ownership of asset but only sends proofs to some State account
    /// </summary>
    public class TransitionOnboardingDisclosingProofs : UtxoConfidentialBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => BlockTypes.UtxoConfidential_TransitionOnboardingDisclosingProofs;

		/// <summary>
		/// C = x * G + I, where I is elliptic curve point representing assert id
		/// </summary>
		public byte[] AssetCommitment { get; set; }

        /// <summary>
        /// Contains encrypted blinding factor of AssetCommitment: x` = x ^ (r * A). To decrypt receiver makes (R * a) ^ x` = x.
        /// </summary>
        public EcdhTupleProofs EcdhTuple { get; set; }

        /// <summary>
        /// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
        /// </summary>
        public SurjectionProof OwnershipProof { get; set; }

        /// <summary>
        /// Contain Surjection Proofs to assets issued by Asset Issuer specified at EcdhTuple for checking eligibility purposes
        /// </summary>
        public SurjectionProof EligibilityProof { get; set; }

		public AssociatedProofs[] AssociatedProofs { get; set; }
	}
}
