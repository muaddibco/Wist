using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.Cryptography
{
    public class RangeProof
    {
        public byte[][] D { get; set; } // N/2 digit Pedersen commitments

        public BorromeanRingSignatureEx BorromeanRingSignature { get; set; }
    }
}
