using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class AcceptedAsset
    {
        public byte[] AssetCommitment { get; set; }

        public SurjectionProof SurjectionProof { get; set; }
    }
}
