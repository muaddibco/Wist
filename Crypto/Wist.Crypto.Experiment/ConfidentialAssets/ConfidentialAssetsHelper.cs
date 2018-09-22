using Wist.Core.ExtensionMethods;
using Chaos.NaCl.Internal.Ed25519Ref10;
using HashLib;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment.ConfidentialAssets
{
    public static class ConfidentialAssetsHelper
    {
        // Inputs:
        //
        // 1. `msg`: the 32-byte string to be signed.
        // 2. `{P[i]}`: `n` public keys, [points](data.md#public-key) on the elliptic curve.
        // 3. `j`: the index of the designated public key, so that `P[j] == p*G`.
        // 4. `p`: the private key for the public key `P[j]`.
        //
        // Output: `{e0, s[0], ..., s[n-1]}`: the ring signature, `n+1` 32-byte elements.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg">32 byte of message to sign</param>
        /// <param name="pks">collection of public key where secret key of one of the is known to signer</param>
        /// <param name="j">index of public key that its secret key is provided in argument "sk"</param>
        /// <param name="sk">secret key for public key with index j</param>
        /// <returns></returns>
        public static RingSignature CreateRingSignature(byte[] msg, byte[][] pks, ulong j, byte[] sk)
        {
            RingSignature ringSignature = null;

            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (pks == null)
            {
                throw new ArgumentNullException(nameof(pks));
            }

            if (sk == null)
            {
                throw new ArgumentNullException(nameof(sk));
            }

            if(pks.Length == 0)
            {
                ringSignature = new RingSignature();
                return ringSignature;
            }

            ulong n = (ulong)pks.Length;
            ringSignature = new RingSignature((int)n);

            // 1. Let `counter = 0`.
            int counter = 0;
            while (true)
            {
                byte[][] e0 = new byte[2][]; // second slot is to put non-zero value in a time-constant manner

                // 2. Calculate a sequence of: `n-1` 32-byte random values, 64-byte `nonce` and 1-byte `mask`:
                //    `{r[i], nonce, mask} = SHAKE256(counter || p || msg, 8*(32*(n-1) + 64 + 1))`,
                //    where `p` is encoded in 32 bytes using little-endian convention, and `counter` is encoded as a 64-bit little-endian integer.
                byte[][] r = new byte[n][];
                byte[] buf;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (BinaryWriter bw = new BinaryWriter(ms))
                    {
                        bw.Write((ulong)counter);
                        bw.Write(msg);
                        bw.Write(sk);
                        bw.Write((ulong)j);
                    }

                    buf = ms.ToArray();
                }

                ShakeDigest shakeDigest = new ShakeDigest(256);
                shakeDigest.BlockUpdate(buf, 0, buf.Length);
                foreach (byte[] pk in pks)
                {
                    shakeDigest.BlockUpdate(pk, 0, pk.Length);
                }

                for (int m = 0; m < (int)n - 1; m++)
                {
                    r[m] = new byte[32];
                    shakeDigest.DoOutput(r[m], 0, 32);
                }

                byte[] nonce = new byte[64];
                byte[] mask = new byte[1];

                shakeDigest.DoOutput(nonce, 0, 64);
                shakeDigest.DoOutput(mask, 0, 1);

                // 3. Calculate `k = nonce mod L`, where `nonce` is interpreted as a 64-byte little-endian integer and reduced modulo subgroup order `L`.
                ScalarOperations.sc_reduce(nonce);

                // 4. Calculate the initial e-value, let `i = j+1 mod n`:
                ulong i = (j + 1L) % n;

                // 4.1. Calculate `R[i]` as the point `k*G`.
                GroupElementP3 Ri;
                GroupOperations.ge_scalarmult_base(out Ri, nonce, 0);

                // 4.2. Define `w[j]` as `mask` with lower 4 bits set to zero: `w[j] = mask & 0xf0`.
                byte wj = (byte)(mask[0] & 0xf0);

                // 4.3. Calculate `e[i] = SHA3-512(R[i] || msg || i)` where `i` is encoded as a 64-bit little-endian integer. Interpret `e[i]` as a little-endian integer reduced modulo `L`.
                byte[] Rienc = new byte[32];
                GroupOperations.ge_p3_tobytes(Rienc, 0, ref Ri);
                IHash hash = HashLib.HashFactory.Crypto.CreateSHA512();
                hash.TransformBytes(Rienc);
                hash.TransformBytes(msg);
                hash.TransformULong(i);

                byte[] ei = hash.ComputeByte(wj).GetBytes();
                if (i == 0)
                {
                    e0[0] = ei;
                }
                else
                {
                    e0[1] = ei;
                }

                // 5. For `step` from `1` to `n-1` (these steps are skipped if `n` equals 1):
                for (ulong step = 1; step < n; step++)
                {
                    // 5.1. Let `i = (j + step) mod n`.
                    i = (j + step) % n;

                    // 5.2. Set the forged s-value `s[i] = r[step-1]`
                    ringSignature.S[i] = new byte[32];
                    Array.Copy(r[step - 1], 0, ringSignature.S[i], 0, 32);

                    // 5.3. Define `z[i]` as `s[i]` with the most significant 4 bits set to zero.
                    byte[] z = ringSignature.S[i];
                    z[31] &= 0x0f;

                    // 5.4. Define `w[i]` as a most significant byte of `s[i]` with lower 4 bits set to zero: `w[i] = s[i][31] & 0xf0`.
                    byte wi = (byte)(ringSignature.S[i][31] & 0xf0);

                    // 5.5. Let `i’ = i+1 mod n`.
                    ulong i1 = (i + 1) % n;

                    // 5.6. Calculate `R[i’] = z[i]*G - e[i]*P[i]` and encode it as a 32-byte public key.
                    byte[] Ri1 = new byte[32];
                    byte[] ei_invert = new byte[32];
                    ScalarOperations.sc_invert(ei_invert, ei);
                    ScalarmulBaseAddKeys2(Ri1, z, ei_invert, pks[i]);

                    // 5.7. Calculate `e[i’] = SHA3-512(R[i’] || msg || i’)` where `i’` is encoded as a 64-bit little-endian integer.
                    // Interpret `e[i’]` as a little-endian integer.
                    hash.Initialize();
                    hash.TransformBytes(Ri1);
                    hash.TransformBytes(msg);
                    hash.TransformULong(i1);

                    ei = hash.ComputeByte(wi).GetBytes();
                    if (i1 == 0)
                    {
                        e0[0] = ei;
                    }
                    else
                    {
                        e0[1] = ei;
                    }
                }

                // 6. Calculate the non-forged `z[j] = k + p*e[j] mod L` and encode it as a 32-byte little-endian integer.
                byte[] zj = new byte[32];
                ScalarOperations.sc_muladd(zj, sk, ei, nonce);

                // 7. If `z[j]` is greater than 2<sup>252</sup>–1, then increment the `counter` and try again from the beginning.
                //    The chance of this happening is below 1 in 2<sup>124</sup>.
                if ((zj[31] & 0xf0) != 0)
                {
                    // We won a lottery and will try again with an incremented counter.
                    counter++;
                }
                else
                {
                    // 8. Define `s[j]` as `z[j]` with 4 high bits set to high 4 bits of the `mask`.
                    zj[31] ^= (byte)(mask[0] & 0xf0); // zj now == sj

                    // Put non-forged s[j] into ringsig
                    Array.Copy(zj, 0, ringSignature.S[j], 0, 32);

                    // Put e[0] inside the ringsig
                    Array.Copy(e0[0], 0, ringSignature.E, 0, 32);

                    break;
                }
            }

            // 9. Return the ring signature `{e[0], s[0], ..., s[n-1]}`, total `n+1` 32-byte elements.
            return ringSignature;
        }

        public static bool VerifyRingSignature(RingSignature ringSignature, byte[] msg, byte[][] pks)
        {
            if(ringSignature.S.Length != pks.Length)
            {
                throw new ArgumentException($"ring size {ringSignature.S.Length} does not equal number of pubkeys {pks.Length}");
            }

            // 1. For each `i` from `0` to `n-1`:
            ulong n = (ulong)pks.Length;
            byte[] e = ringSignature.E;

            for (ulong i = 0; i < n; i++)
            {
                // 1. Define `z[i]` as `s[i]` with the most significant 4 bits set to zero (see note below).
                byte[] z = new byte[32];
                Array.Copy(ringSignature.S[i], 0, z, 0, 32);
                z[31] &= 0x0f;

                // 2. Define `w[i]` as a most significant byte of `s[i]` with lower 4 bits set to zero: `w[i] = s[i][31] & 0xf0`.
                byte w = (byte)(ringSignature.S[i][31] & 0xf0);

                // 3. Calculate `R[i+1] = z[i]*G - e[i]*P[i]` and encode it as a 32-byte public key.
                byte[] R = new byte[32];
                byte[] e_invert = new byte[32];
                ScalarOperations.sc_invert(e_invert, e);
                ScalarmulBaseAddKeys2(R, z, e_invert, pks[i]);

                // 4. Calculate `e[i+1] = SHA3-512(R[i+1] || msg || i+1)` where `i+1` is encoded as a 64-bit little-endian integer.
                // 5. Interpret `e[i+1]` as a little-endian integer reduced modulo subgroup order `L`.
                IHash hash = HashFactory.Crypto.CreateSHA512();
                hash.TransformBytes(R);
                hash.TransformBytes(msg);
                hash.TransformULong((i + 1) % n);
                e = hash.ComputeByte(w).GetBytes();
            }

            return e.Equals32(ringSignature.E);
        }

        //aGbB = aG + bB where a, b are scalars, G is the basepoint and B is a point
        private static void ScalarmulBaseAddKeys2(byte[] aGbB, byte[] a, byte[] b, byte[] bPoint)
        {
            if (aGbB == null)
            {
                throw new ArgumentNullException(nameof(aGbB));
            }

            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            if (b == null)
            {
                throw new ArgumentNullException(nameof(b));
            }

            if (bPoint == null)
            {
                throw new ArgumentNullException(nameof(bPoint));
            }

            GroupElementP2 rv;
            GroupElementP3 bPointP3;
            if (GroupOperations.ge_frombytes_negate_vartime(out bPointP3, bPoint, 0) != 0)
            {
                throw new ArgumentException(nameof(bPoint), $"Failed to convert to {nameof(GroupElementP3)}");
            }
            GroupOperations.ge_double_scalarmult_vartime(out rv, a, ref bPointP3, b);
            GroupOperations.ge_tobytes(aGbB, 0, ref rv);
        }
    }
}
