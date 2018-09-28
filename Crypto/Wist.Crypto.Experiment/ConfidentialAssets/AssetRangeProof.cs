using Chaos.NaCl.Internal.Ed25519Ref10;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment.ConfidentialAssets
{
    public class AssetRangeProof
    {
        private GroupElementP3[] _h;
        private RingSignature _rs;

        public AssetRangeProof()
        {
            _h = new GroupElementP3[0];
            _rs = new RingSignature();
        }

        internal GroupElementP3[] H { get => _h; set => _h = value; }
        public RingSignature Rs { get => _rs; set => _rs = value; }
    }
}
