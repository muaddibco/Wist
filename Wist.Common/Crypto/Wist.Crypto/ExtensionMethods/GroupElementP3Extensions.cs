using Chaos.NaCl.Internal.Ed25519Ref10;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Crypto.ExtensionMethods
{
    internal static class GroupElementP3Extensions
    {
        internal static byte[] ToBytes(this GroupElementP3 p3)
        {
            byte[] res = new byte[32];
            GroupOperations.ge_p3_tobytes(res, 0, ref p3);

            return res;
        }
    }
}
