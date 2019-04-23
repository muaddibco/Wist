using Chaos.NaCl.Internal.Ed25519Ref10;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment.ConfidentialAssets
{
    public class SurjectionProof
    {
        private GroupElementP3[] _h;
        private BorromeanRingSignature _rs;

        public SurjectionProof()
        {
            _h = new GroupElementP3[0];
            _rs = new BorromeanRingSignature();
        }

        internal GroupElementP3[] H { get => _h; set => _h = value; }
        public BorromeanRingSignature Rs { get => _rs; set => _rs = value; }
    }
}
