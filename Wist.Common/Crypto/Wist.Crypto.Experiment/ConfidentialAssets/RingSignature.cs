using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment.ConfidentialAssets
{
    public class RingSignature
    {
        byte[] _e;
        byte[][] _s;

        public RingSignature()
        {
            _e = new byte[32];
            _s = new byte[0][];
        }

        public RingSignature(int sLength)
        {
            _e = new byte[32];
            _s = new byte[sLength][];

            for (int i = 0; i < sLength; i++)
            {
                _s[i] = new byte[32];
            }
        }

        public byte[] E { get => _e; set => _e = value; }
        public byte[][] S { get => _s; set => _s = value; }
    }
}
