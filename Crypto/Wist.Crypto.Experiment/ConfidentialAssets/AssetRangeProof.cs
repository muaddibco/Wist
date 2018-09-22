using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment.ConfidentialAssets
{
    public class AssetRangeProof
    {
        private byte[][] _h;
        private RingSignature _rs;

        public AssetRangeProof()
        {
            _h = new byte[0][];
            _rs = new RingSignature();
        }

        public byte[][] H { get => _h; set => _h = value; }
        public RingSignature Rs { get => _rs; set => _rs = value; }
    }
}
