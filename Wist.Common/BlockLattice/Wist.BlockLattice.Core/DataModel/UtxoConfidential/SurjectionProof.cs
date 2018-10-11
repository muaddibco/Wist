using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel.UtxoConfidential
{
    public class SurjectionProof
    {
        private byte[][] _h;
        private RingSignature _rs;

        public SurjectionProof()
        {
            _h = new byte[0][];
            _rs = new RingSignature();
        }

        internal byte[][] H { get => _h; set => _h = value; }

        public RingSignature Rs { get => _rs; set => _rs = value; }
    }
}
