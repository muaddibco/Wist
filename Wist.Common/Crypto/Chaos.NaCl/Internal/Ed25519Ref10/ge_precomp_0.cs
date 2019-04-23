using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
    internal static partial class GroupOperations
    {
        public static void ge_precomp_0(out GroupElementPreComp h)
        {
            FieldOperations.fe_1(out h.yplusx);
            FieldOperations.fe_1(out h.yminusx);
            FieldOperations.fe_0(out h.xy2d);
        }

        public static void ge_dsm_precomp(GroupElementCached[] r, ref GroupElementP3 s)
        {
            if (r == null)
            {
                throw new ArgumentNullException(nameof(r));
            }

            if(r.Length != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(r), "Expected exactly 8 items");
            }

            ge_p3_to_cached(out r[0], ref s);
            ge_p3_dbl(out GroupElementP1P1 t, ref s);
            ge_p1p1_to_p3(out GroupElementP3 s2, ref t);
            ge_add(out t, ref s2, ref r[0]); ge_p1p1_to_p3(out GroupElementP3 u, ref t); ge_p3_to_cached(out r[1], ref u);
            ge_add(out t, ref s2, ref r[1]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out r[2], ref u);
            ge_add(out t, ref s2, ref r[2]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out r[3], ref u);
            ge_add(out t, ref s2, ref r[3]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out r[4], ref u);
            ge_add(out t, ref s2, ref r[4]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out r[5], ref u);
            ge_add(out t, ref s2, ref r[5]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out r[6], ref u);
            ge_add(out t, ref s2, ref r[6]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out r[7], ref u);
        }
    }
}