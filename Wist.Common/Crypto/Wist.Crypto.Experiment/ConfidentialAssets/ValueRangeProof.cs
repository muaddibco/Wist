using Chaos.NaCl.Internal.Ed25519Ref10;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment.ConfidentialAssets
{
    internal class ValueRangeProof
    {
        public byte[][] D { get; set; } // N/2 digit Pedersen commitments
        
        internal BorromeanRingSignatureEx BorromeanRingSignature { get; set; }
    }
}
