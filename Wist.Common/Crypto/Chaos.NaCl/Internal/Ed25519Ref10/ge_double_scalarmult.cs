using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
    internal static partial class GroupOperations
    {
        private static void slide(sbyte[] r, byte[] a)
        {
            int i;
            int b;
            int k;

            for (i = 0; i < 256; ++i)
                r[i] = (sbyte)(1 & (a[i >> 3] >> (i & 7)));

            for (i = 0; i < 256; ++i)
                if (r[i] != 0)
                {
                    for (b = 1; b <= 6 && i + b < 256; ++b)
                    {
                        if (r[i + b] != 0)
                        {
                            if (r[i] + (r[i + b] << b) <= 15)
                            {
                                r[i] += (sbyte)(r[i + b] << b); r[i + b] = 0;
                            }
                            else if (r[i] - (r[i + b] << b) >= -15)
                            {
                                r[i] -= (sbyte)(r[i + b] << b);
                                for (k = i + b; k < 256; ++k)
                                {
                                    if (r[k] == 0)
                                    {
                                        r[k] = 1;
                                        break;
                                    }
                                    r[k] = 0;
                                }
                            }
                            else
                                break;
                        }
                    }
                }

        }

        /*
		r = a * A + b * B
		where a = a[0]+256*a[1]+...+256^31 a[31].
		and b = b[0]+256*b[1]+...+256^31 b[31].
		B is the Ed25519 base point (x,4/5) with x positive.
		*/
        /// <summary>
        /// r = a * A + b * B, B is the Ed25519 base point (x,4/5) with x positive.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="a">a = a[0]+256*a[1]+...+256^31 a[31]</param>
        /// <param name="A"></param>
        /// <param name="b">b = b[0]+256*b[1]+...+256^31 b[31]</param>
        public static void ge_double_scalarmult_vartime(out GroupElementP2 r, byte[] a, ref GroupElementP3 A, byte[] b)
        {
            GroupElementPreComp[] Bi = LookupTables.Base2;
            //TODO: Perhaps remove these allocations?
            sbyte[] aslide = new sbyte[256];
            sbyte[] bslide = new sbyte[256];
            GroupElementCached[] Ai = new GroupElementCached[8]; /* A,3A,5A,7A,9A,11A,13A,15A */
            GroupElementP1P1 t;
            GroupElementP3 u;
            int i;

            slide(aslide, a);
            slide(bslide, b);

            ge_dsm_precomp(Ai, ref A);

            ge_p2_0(out r);

            for (i = 255; i >= 0; --i)
            {
                if ((aslide[i] != 0) || (bslide[i] != 0)) break;
            }

            for (; i >= 0; --i)
            {
                ge_p2_dbl(out t, ref r);

                if (aslide[i] > 0)
                {
                    ge_p1p1_to_p3(out u, ref t);
                    ge_add(out t, ref u, ref Ai[aslide[i] / 2]);
                }
                else if (aslide[i] < 0)
                {
                    ge_p1p1_to_p3(out u, ref t);
                    ge_sub(out t, ref u, ref Ai[(-aslide[i]) / 2]);
                }

                if (bslide[i] > 0)
                {
                    ge_p1p1_to_p3(out u, ref t);
                    ge_madd(out t, ref u, ref Bi[bslide[i] / 2]);
                }
                else if (bslide[i] < 0)
                {
                    ge_p1p1_to_p3(out u, ref t);
                    ge_msub(out t, ref u, ref Bi[(-bslide[i]) / 2]);
                }

                ge_p1p1_to_p2(out r, ref t);
            }
        }

        /* Assumes that a[31] <= 127 */
        public static void ge_scalarmult(out GroupElementP2 r, byte[] a, ref GroupElementP3 A)
        {
            sbyte[] e = new sbyte[64];
            int carry, carry2, i;
            GroupElementCached[] Ai = new GroupElementCached[8]; /* 1 * A, 2 * A, ..., 8 * A */
            GroupElementP1P1 t;
            GroupElementP3 u;

            unchecked
            {
                carry = 0; /* 0..1 */
                for (i = 0; i < 31; i++)
                {
                    carry += a[i]; /* 0..256 */
                    carry2 = (carry + 8) >> 4; /* 0..16 */
                    e[2 * i] = (sbyte)(carry - (carry2 << 4)); /* -8..7 */
                    carry = (carry2 + 8) >> 4; /* 0..1 */
                    e[2 * i + 1] = (sbyte)(carry2 - (carry << 4)); /* -8..7 */
                }
                carry += a[31]; /* 0..128 */
                carry2 = (carry + 8) >> 4; /* 0..8 */
                e[62] = (sbyte)(carry - (carry2 << 4)); /* -8..7 */
                e[63] = (sbyte)carry2; /* 0..8 */
            }

            ge_p3_to_cached(out Ai[0], ref A);
            for (i = 0; i < 7; i++)
            {
                ge_add(out t, ref A, ref Ai[i]);
                ge_p1p1_to_p3(out u, ref t);
                ge_p3_to_cached(out Ai[i + 1], ref u);
            }

            ge_p2_0(out r);
            for (i = 63; i >= 0; i--)
            {
                sbyte b = e[i];
                byte bnegative = negative(b);
                byte babs = (byte)(b - (((-bnegative) & b) << 1));
                GroupElementCached cur, minuscur = new GroupElementCached();
                ge_p2_dbl(out t, ref r);
                ge_p1p1_to_p2(out r, ref t);
                ge_p2_dbl(out t, ref r);
                ge_p1p1_to_p2(out r, ref t);
                ge_p2_dbl(out t, ref r);
                ge_p1p1_to_p2(out r, ref t);
                ge_p2_dbl(out t, ref r);
                ge_p1p1_to_p3(out u, ref t);
                ge_cached_0(out cur);
                ge_cached_cmov(ref cur, ref Ai[0], equal(babs, 1));
                ge_cached_cmov(ref cur, ref Ai[1], equal(babs, 2));
                ge_cached_cmov(ref cur, ref Ai[2], equal(babs, 3));
                ge_cached_cmov(ref cur, ref Ai[3], equal(babs, 4));
                ge_cached_cmov(ref cur, ref Ai[4], equal(babs, 5));
                ge_cached_cmov(ref cur, ref Ai[5], equal(babs, 6));
                ge_cached_cmov(ref cur, ref Ai[6], equal(babs, 7));
                ge_cached_cmov(ref cur, ref Ai[7], equal(babs, 8));
                FieldOperations.fe_copy(out minuscur.YplusX, ref cur.YminusX);
                FieldOperations.fe_copy(out minuscur.YminusX, ref cur.YplusX);
                FieldOperations.fe_copy(out minuscur.Z, ref cur.Z);
                FieldOperations.fe_neg(out minuscur.T2d, ref cur.T2d);
                ge_cached_cmov(ref cur, ref minuscur, bnegative);
                ge_add(out t, ref u, ref cur);
                ge_p1p1_to_p2(out r, ref t);
            }
        }

        public static void ge_scalarmult_p3(out GroupElementP3 r3, byte[] a, ref GroupElementP3 A)
        {
            sbyte[] e = new sbyte[64];
            int carry, carry2, i;
            GroupElementCached[] Ai = new GroupElementCached[8]; /* 1 * A, 2 * A, ..., 8 * A */
            GroupElementP1P1 t;
            GroupElementP3 u;
            GroupElementP2 r;

            carry = 0; /* 0..1 */
            for (i = 0; i < 31; i++)
            {
                carry += a[i]; /* 0..256 */
                carry2 = (carry + 8) >> 4; /* 0..16 */
                e[2 * i] = (sbyte)(carry - (carry2 << 4)); /* -8..7 */
                carry = (carry2 + 8) >> 4; /* 0..1 */
                e[2 * i + 1] = (sbyte)(carry2 - (carry << 4)); /* -8..7 */
            }
            carry += a[31]; /* 0..128 */
            carry2 = (carry + 8) >> 4; /* 0..8 */
            e[62] = (sbyte)(carry - (carry2 << 4)); /* -8..7 */
            e[63] = (sbyte)carry2; /* 0..8 */

            ge_p3_to_cached(out Ai[0], ref A);
            for (i = 0; i < 7; i++)
            {
                ge_add(out t, ref A, ref Ai[i]);
                ge_p1p1_to_p3(out u, ref t);
                ge_p3_to_cached(out Ai[i + 1], ref u);
            }

            ge_p2_0(out r);
            GroupElementP3 resP3;
            ge_p3_0(out resP3);
            for (i = 63; i >= 0; i--)
            {
                sbyte b = e[i];
                byte bnegative = negative(b);
                byte babs = (byte)(b - (((-bnegative) & b) << 1));
                GroupElementCached cur, minuscur;
                ge_p2_dbl(out t, ref r);
                ge_p1p1_to_p2(out r, ref t);
                ge_p2_dbl(out t, ref r);
                ge_p1p1_to_p2(out r, ref t);
                ge_p2_dbl(out t, ref r);
                ge_p1p1_to_p2(out r, ref t);
                ge_p2_dbl(out t, ref r);
                ge_p1p1_to_p3(out u, ref t);
                ge_cached_0(out cur);
                ge_cached_cmov(ref cur, ref Ai[0], equal(babs, 1));
                ge_cached_cmov(ref cur, ref Ai[1], equal(babs, 2));
                ge_cached_cmov(ref cur, ref Ai[2], equal(babs, 3));
                ge_cached_cmov(ref cur, ref Ai[3], equal(babs, 4));
                ge_cached_cmov(ref cur, ref Ai[4], equal(babs, 5));
                ge_cached_cmov(ref cur, ref Ai[5], equal(babs, 6));
                ge_cached_cmov(ref cur, ref Ai[6], equal(babs, 7));
                ge_cached_cmov(ref cur, ref Ai[7], equal(babs, 8));
                FieldOperations.fe_copy(out minuscur.YplusX, ref cur.YminusX);
                FieldOperations.fe_copy(out minuscur.YminusX, ref cur.YplusX);
                FieldOperations.fe_copy(out minuscur.Z, ref cur.Z);
                FieldOperations.fe_neg(out minuscur.T2d, ref cur.T2d);
                ge_cached_cmov(ref cur, ref minuscur, bnegative);
                ge_add(out t, ref u, ref cur);
                if (i == 0)
                    ge_p1p1_to_p3(out resP3, ref t);
                else
                    ge_p1p1_to_p2(out r, ref t);
            }

            r3 = resP3;
        }

        public static void ge_double_scalarmult_precomp_vartime(out GroupElementP2 r, byte[] a, GroupElementP3 A, byte[] b, GroupElementCached[] Bi)
        {

            GroupElementCached[] Ai = new GroupElementCached[8]; /* A, 3A, 5A, 7A, 9A, 11A, 13A, 15A */

            ge_dsm_precomp(Ai, ref A);
            ge_double_scalarmult_precomp_vartime2(out r, a, Ai, b, Bi);
        }

        public static void ge_double_scalarmult_precomp_vartime2(out GroupElementP2 r, byte[] a, GroupElementCached[] Ai, byte[] b, GroupElementCached[] Bi)
        {
            sbyte[] aslide = new sbyte[256];
            sbyte[] bslide = new sbyte[256];
            GroupElementP1P1 t;
            GroupElementP3 u;
            int i;

            slide(aslide, a);
            slide(bslide, b);

            ge_p2_0(out r);

            for (i = 255; i >= 0; --i)
            {
                if ((aslide[i] != 0) || (bslide[i] != 0)) break;
            }

            for (; i >= 0; --i)
            {
                ge_p2_dbl(out t, ref r);

                if (aslide[i] > 0)
                {
                    ge_p1p1_to_p3(out u, ref t);
                    ge_add(out t, ref u, ref Ai[aslide[i] / 2]);
                }
                else if (aslide[i] < 0)
                {
                    ge_p1p1_to_p3(out u, ref t);
                    ge_sub(out t, ref u, ref Ai[(-aslide[i]) / 2]);
                }

                if (bslide[i] > 0)
                {
                    ge_p1p1_to_p3(out u, ref t);
                    ge_add(out t, ref u, ref Bi[bslide[i] / 2]);
                }
                else if (bslide[i] < 0)
                {
                    ge_p1p1_to_p3(out u, ref t);
                    ge_sub(out t, ref u, ref Bi[(-bslide[i]) / 2]);
                }

                ge_p1p1_to_p2(out r, ref t);
            }
        }

        static void ge_cached_0(out GroupElementCached r)
        {
            FieldOperations.fe_1(out r.YplusX);
            FieldOperations.fe_1(out r.YminusX);
            FieldOperations.fe_1(out r.Z);
            FieldOperations.fe_0(out r.T2d);
        }

        static void ge_cached_cmov(ref GroupElementCached t, ref GroupElementCached u, byte b)
        {
            FieldOperations.fe_cmov(ref t.YplusX, ref u.YplusX, b);
            FieldOperations.fe_cmov(ref t.YminusX, ref u.YminusX, b);
            FieldOperations.fe_cmov(ref t.Z, ref u.Z, b);
            FieldOperations.fe_cmov(ref t.T2d, ref u.T2d, b);
        }
    }
}