using System;
using System.Collections.Generic;
using System.Text;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
    internal static partial class ScalarOperations
    {
        internal static void sc_invert(byte[] s, byte[] a)
        {
            sc_muladd(s, a, negone, zero);
        }
    }
}
