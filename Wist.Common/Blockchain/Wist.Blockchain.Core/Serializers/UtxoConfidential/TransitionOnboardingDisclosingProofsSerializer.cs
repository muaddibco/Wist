using System.IO;
using Wist.Blockchain.Core.DataModel.UtxoConfidential;
using Wist.Blockchain.Core.DataModel.UtxoConfidential.Internal;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Serializers.UtxoConfidential
{
	[RegisterExtension(typeof(ISerializer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class TransitionOnboardingDisclosingProofsSerializer : UtxoConfidentialSerializerBase<TransitionOnboardingDisclosingProofs>
    {
        public TransitionOnboardingDisclosingProofsSerializer() : base(PacketType.UtxoConfidential, BlockTypes.UtxoConfidential_TransitionOnboardingDisclosingProofs)
        {
        }

        protected override void WriteBody(BinaryWriter bw)
        {
            bw.Write(_block.AssetCommitment);
			WriteEcdhTupleProofs(bw, _block.EcdhTuple);
            WriteSurjectionProof(bw, _block.OwnershipProof);
            WriteSurjectionProof(bw, _block.EligibilityProof);

			if(_block.AssociatedProofs != null && _block.AssociatedProofs.Length > 0)
			{
				bw.Write((byte)_block.AssociatedProofs.Length);

				foreach (var item in _block.AssociatedProofs)
				{
					if(item is AssociatedAssetProofs associatedAssetProofs)
					{
						bw.Write((byte)1);
						bw.Write(associatedAssetProofs.AssociatedAssetCommitment);
					}
					else
					{
						bw.Write((byte)0);
					}

					bw.Write(item.AssociatedAssetGroupId);
					WriteSurjectionProof(bw, item.AssociationProofs);
					WriteSurjectionProof(bw, item.RootProofs);
				}
			}
			else
			{
				bw.Write((byte)0);
			}
        }
    }
}
