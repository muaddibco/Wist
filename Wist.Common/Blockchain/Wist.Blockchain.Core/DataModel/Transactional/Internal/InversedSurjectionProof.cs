using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Cryptography;

namespace Wist.Blockchain.Core.DataModel.Transactional.Internal
{
    public class InversedSurjectionProof
    {
        public InversedSurjectionProof()
        {
            Rs = new BorromeanRingSignature();
        }

        public byte[] AssetCommitment { get; set; }
        public BorromeanRingSignature Rs { get; set; }
    }
}
