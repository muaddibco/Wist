using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.DataModel.UtxoConfidential.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.UtxoConfidential
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class TransitionOnboardingDisclosingProofsParser : UtxoConfidentialParserBase
    {
        public TransitionOnboardingDisclosingProofsParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.UtxoConfidential_TransitionOnboardingDisclosingProofs;

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

				byte associatedProofsCount = spanBody.Slice(readBytes++).Span[0];

				AssociatedProofs[] associatedProofs = new AssociatedProofs[associatedProofsCount];

				for (int i = 0; i < associatedProofsCount; i++)
				{
					byte associatedProofType = spanBody.Slice(readBytes++).Span[0];

					AssociatedProofs associatedProof;

					if (associatedProofType == 1)
					{
						ReadCommitment(ref spanBody, ref readBytes, out byte[] associatedAssetCommitment);
						associatedProof = new AssociatedAssetProofs
						{
							AssociatedAssetCommitment = associatedAssetCommitment
						};
					}
					else
					{
						associatedProof = new AssociatedProofs();
					}

					ReadCommitment(ref spanBody, ref readBytes, out byte[] associatedGroupId);
					ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof associationProofs);
					ReadSurjectionProof(ref spanBody, ref readBytes, out SurjectionProof rootProofs);

					associatedProof.AssociatedAssetGroupId = associatedGroupId;
					associatedProof.AssociationProofs = associationProofs;
					associatedProof.RootProofs = rootProofs;

					associatedProofs[i] = associatedProof;
				}

				block = new TransitionOnboardingDisclosingProofs
                {
					AssetCommitment = assetCommitment,
                    EcdhTuple = ecdhTuple,
                    OwnershipProof = ownershipProofs,
                    EligibilityProof = eligibilityProofs,
					AssociatedProofs = associatedProofs
				};

                utxoConfidentialBase = block;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
