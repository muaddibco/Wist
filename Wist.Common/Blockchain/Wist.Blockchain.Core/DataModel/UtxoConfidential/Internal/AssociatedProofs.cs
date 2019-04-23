using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.UtxoConfidential.Internal
{
	public class AssociatedProofs
	{
		public byte[] AssociatedAssetGroupId { get; set; }
		public SurjectionProof AssociationProofs { get; set; }
		public SurjectionProof RootProofs { get; set; }
	}
}
