using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.UtxoConfidential
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
	public class TransitionAuthenticationProofsSerializer : UtxoConfidentialSerializerBase<TransitionAuthenticationProofs>
	{
		public TransitionAuthenticationProofsSerializer() : base(PacketType.UtxoConfidential, BlockTypes.UtxoConfidential_TransitionAuthenticationProofs)
		{
		}

		protected override void WriteBody(BinaryWriter bw)
		{
			bw.Write(_block.AssetCommitment);
			WriteEcdhTupleProofs(bw, _block.EncodedPayload);
			WriteSurjectionProof(bw, _block.OwnershipProof);
			WriteSurjectionProof(bw, _block.EligibilityProof);
			WriteSurjectionProof(bw, _block.AuthenticationProof);
		}
	}
}
