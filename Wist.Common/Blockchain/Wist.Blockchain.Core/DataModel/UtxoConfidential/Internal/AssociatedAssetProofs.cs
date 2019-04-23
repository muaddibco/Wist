using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.UtxoConfidential.Internal
{
	public class AssociatedAssetProofs : AssociatedProofs
	{
		public byte[] AssociatedAssetCommitment { get; set; }
	}
}
