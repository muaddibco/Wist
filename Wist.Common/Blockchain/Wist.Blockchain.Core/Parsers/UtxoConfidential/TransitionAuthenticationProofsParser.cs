using System;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.UtxoConfidential
{
	[RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
	public class TransitionAuthenticationProofsParser : UtxoConfidentialParserBase
	{
		public TransitionAuthenticationProofsParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
		{
		}

		public override ushort BlockType => BlockTypes.UtxoConfidential_TransitionAuthenticationProofs;

		protected override Memory<byte> ParseUtxoConfidential(ushort version, Memory<byte> spanBody, out UtxoConfidentialBase utxoConfidentialBase)
		{
			UtxoConfidentialBase block = null;

			if (version == 1)
			{
				int readBytes = 0;

				ReadCommitment(ref spanBody, ref readBytes, out byte[] assetCommitment);
				ReadEcdhTupleProofs(ref spanBody, ref readBytes, out EcdhTupleProofs ecdhTuple);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof ownershipProofs);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof eligibilityProofs);
				ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof authenticationProofs);

				block = new TransitionAuthenticationProofs
				{
					AssetCommitment = assetCommitment,
					EncodedPayload = ecdhTuple,
					OwnershipProof = ownershipProofs,
					EligibilityProof = eligibilityProofs,
					AuthenticationProof = authenticationProofs
				};

				utxoConfidentialBase = block;

				return spanBody.Slice(readBytes);
			}

			throw new BlockVersionNotSupportedException(version, BlockType);
		}
	}
}
