using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
    internal static partial class GroupOperations
    {
        public static int ge_frombytes_negate_vartime(out GroupElementP3 h, byte[] data, int offset)
        {
            FieldElement u;
            FieldElement v;
            FieldElement v3;
            FieldElement vxx;
            FieldElement check;

            FieldOperations.fe_frombytes(out h.Y, data, offset);
            FieldOperations.fe_1(out h.Z);
            FieldOperations.fe_sq(out u, ref h.Y);
            FieldOperations.fe_mul(out v, ref u, ref LookupTables.d);
            FieldOperations.fe_sub(out u, ref u, ref h.Z);       /* u = y^2-1 */
            FieldOperations.fe_add(out v, ref v, ref h.Z);       /* v = dy^2+1 */

            FieldOperations.fe_sq(out v3, ref v);
            FieldOperations.fe_mul(out v3, ref v3, ref v);        /* v3 = v^3 */
            FieldOperations.fe_sq(out h.X, ref v3);
            FieldOperations.fe_mul(out h.X, ref h.X, ref v);
            FieldOperations.fe_mul(out h.X, ref h.X, ref u);    /* x = uv^7 */

            FieldOperations.fe_pow22523(out h.X, ref h.X); /* x = (uv^7)^((q-5)/8) */
            FieldOperations.fe_mul(out h.X, ref h.X, ref v3);
            FieldOperations.fe_mul(out h.X, ref h.X, ref u);    /* x = uv^3(uv^7)^((q-5)/8) */

            FieldOperations.fe_sq(out vxx, ref h.X);
            FieldOperations.fe_mul(out vxx, ref vxx, ref v);
            FieldOperations.fe_sub(out check, ref vxx, ref u);    /* vx^2-u */
            if (FieldOperations.fe_isnonzero(ref check) != 0)
            {
                FieldOperations.fe_add(out check, ref vxx, ref u);  /* vx^2+u */
                if (FieldOperations.fe_isnonzero(ref check) != 0)
                {
                    h = default(GroupElementP3);
                    return -1;
                }
                FieldOperations.fe_mul(out h.X, ref h.X, ref LookupTables.sqrtm1);
            }

            if (FieldOperations.fe_isnegative(ref h.X) == (data[offset + 31] >> 7))
                FieldOperations.fe_neg(out h.X, ref h.X);

            FieldOperations.fe_mul(out h.T, ref h.X, ref h.Y);
            return 0;
        }

        public static int ge_frombytes(out GroupElementP3 h, byte[] data, int offset)
        {
            FieldElement u;
            FieldElement v;
            FieldElement v3;
            FieldElement vxx;
            FieldElement check;

            FieldOperations.fe_frombytes(out h.Y, data, offset);
            FieldOperations.fe_1(out h.Z);
            FieldOperations.fe_sq(out u, ref h.Y);
            FieldOperations.fe_mul(out v, ref u, ref LookupTables.d);
            FieldOperations.fe_sub(out u, ref u, ref h.Z);       /* u = y^2-1 */
            FieldOperations.fe_add(out v, ref v, ref h.Z);       /* v = dy^2+1 */

            FieldOperations.fe_sq(out v3, ref v);
            FieldOperations.fe_mul(out v3, ref v3, ref v);        /* v3 = v^3 */
            FieldOperations.fe_sq(out h.X, ref v3);
            FieldOperations.fe_mul(out h.X, ref h.X, ref v);
            FieldOperations.fe_mul(out h.X, ref h.X, ref u);    /* x = uv^7 */

            FieldOperations.fe_pow22523(out h.X, ref h.X); /* x = (uv^7)^((q-5)/8) */
            FieldOperations.fe_mul(out h.X, ref h.X, ref v3);
            FieldOperations.fe_mul(out h.X, ref h.X, ref u);    /* x = uv^3(uv^7)^((q-5)/8) */

            FieldOperations.fe_sq(out vxx, ref h.X);
            FieldOperations.fe_mul(out vxx, ref vxx, ref v);
            FieldOperations.fe_sub(out check, ref vxx, ref u);    /* vx^2-u */
            if (FieldOperations.fe_isnonzero(ref check) != 0)
            {
                FieldOperations.fe_add(out check, ref vxx, ref u);  /* vx^2+u */
                if (FieldOperations.fe_isnonzero(ref check) != 0)
                {
                    h = default(GroupElementP3);
                    return -1;
                }
                FieldOperations.fe_mul(out h.X, ref h.X, ref LookupTables.sqrtm1);
                
                FieldOperations.fe_reduce(out h.X, ref h.X); 
            }

            if (FieldOperations.fe_isnegative(ref h.X) != (data[offset + 31] >> 7))
                FieldOperations.fe_neg(out h.X, ref h.X);

            FieldOperations.fe_mul(out h.T, ref h.X, ref h.Y);
            return 0;
        }

        public static void ge_fromfe_frombytes_vartime(out GroupElementP2 r, byte[] s, int offset)
        {
            FieldElement u, v, w, x, y, z;
            byte sign;

            FieldOperations.fe_frombytes(out u, s, offset);
            FieldOperations.fe_sq2(out v, ref u); /* 2 * u^2 */

            FieldOperations.fe_1(out w);
            FieldOperations.fe_add(out w, ref v, ref w); /* w = 2 * u^2 + 1 */
            FieldOperations.fe_sq(out x, ref w); /* w^2 */
            FieldOperations.fe_mul(out y, ref FieldOperations.fe_ma2, ref v); /* -2 * A^2 * u^2 */
            FieldOperations.fe_add(out x, ref x, ref y); /* x = w^2 - 2 * A^2 * u^2 */
            FieldOperations.fe_divpowm1(out r.X, ref w, ref x); /* (w / x)^(m + 1) */
            FieldOperations.fe_sq(out y, ref r.X);
            FieldOperations.fe_mul(out x, ref y, ref x);
            FieldOperations.fe_sub(out y, ref w, ref x);
            FieldOperations.fe_copy(out z, ref FieldOperations.fe_ma);
            if (FieldOperations.fe_isnonzero(ref y) != 0)
            {
                FieldOperations.fe_add(out y, ref w, ref x);
                if (FieldOperations.fe_isnonzero(ref y) != 0)
                {
                    goto negative;
                }
                else
                {
                    FieldOperations.fe_mul(out r.X, ref r.X, ref FieldOperations.fe_fffb1);
                }
            }
            else
            {
                FieldOperations.fe_mul(out r.X, ref r.X, ref FieldOperations.fe_fffb2);
            }
            FieldOperations.fe_mul(out r.X, ref r.X, ref u); /* u * sqrt(2 * A * (A + 2) * w / x) */
            FieldOperations.fe_mul(out z, ref z, ref v); /* -2 * A * u^2 */
            sign = 0;
            goto setsign;
            negative:
            FieldOperations.fe_mul(out x, ref x, ref FieldOperations.fe_sqrtm1);
            FieldOperations.fe_sub(out y, ref w, ref x);
            if (FieldOperations.fe_isnonzero(ref y) != 0)
            {
                //assert((fe_add(y, w, x), !fe_isnonzero(y)));
                FieldOperations.fe_mul(out r.X, ref r.X, ref FieldOperations.fe_fffb3);
            }
            else
            {
                FieldOperations.fe_mul(out r.X, ref r.X, ref FieldOperations.fe_fffb4);
            }
            /* r->X = sqrt(A * (A + 2) * w / x) */
            /* z = -A */
            sign = 1;
            setsign:
            if (FieldOperations.fe_isnegative(ref r.X) != sign)
            {
                //assert(fe_isnonzero(r->X));
                FieldOperations.fe_neg(out r.X, ref r.X);
            }
            FieldOperations.fe_add(out r.Z, ref z, ref w);
            FieldOperations.fe_sub(out r.Y, ref z, ref w);
            FieldOperations.fe_mul(out r.X, ref r.X, ref r.Z);
        }
    }
}