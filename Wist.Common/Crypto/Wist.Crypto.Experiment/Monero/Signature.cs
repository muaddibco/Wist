using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment.Monero
{
    public class Signature
    {
        public Signature()
        {
            C = new byte[32];
            R = new byte[32];
        }

        public byte[] C { get; set; }

        public byte[] R { get; set; }
    }
}
