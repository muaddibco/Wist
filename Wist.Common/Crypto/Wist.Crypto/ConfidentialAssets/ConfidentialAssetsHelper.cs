using Chaos.NaCl.Internal.Ed25519Ref10;
using HashLib;
using System;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Crypto.ExtensionMethods;

namespace Wist.Crypto.ConfidentialAssets
{
    public static class ConfidentialAssetsHelper
    {
        public static AssetRangeProof CreateAssetRangeProof(byte[] assetCommitment, byte[][] candidateAssetCommitments, int index, byte[] blindingFactor)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitmentP3, assetCommitment, 0);

            GroupElementP3[] candidateAssetCommitmentsP3 = TranslatePoints(candidateAssetCommitments);

            RingSignature ringSignature = CreateAssetRangeProof(assetCommitmentP3, candidateAssetCommitmentsP3, index, blindingFactor);

            AssetRangeProof assetRangeProof = new AssetRangeProof
            {
                H = candidateAssetCommitments,
                Rs = ringSignature
            };

            return assetRangeProof;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetCommitment">Asset Commitment being sent to recipient</param>
        /// <param name="encryptedAssetID">Encrypted Asset Id being sent to recipient</param>
        /// <param name="candidateAssetCommitments"></param>
        /// <param name="j">index of input commitment among all input commitments that belong to sender and transferred to recipient</param>
        /// <param name="blindingFactor">Blinding factor used for creation Asset Commitment being sent to recipient</param>
        /// <returns></returns>
        internal static RingSignature CreateAssetRangeProof(GroupElementP3 assetCommitment, GroupElementP3[] candidateAssetCommitments, int j, byte[] blindingFactor)
        {
            byte[] msg = CalcAssetRangeProofMsg(assetCommitment, candidateAssetCommitments);
            GroupElementP3[] pubkeys = CalcAssetRangeProofPubkeys(assetCommitment, candidateAssetCommitments);

            RingSignature ringSignature = CreateRingSignature(msg, pubkeys, j, blindingFactor);

            return ringSignature;
        }

        public static bool VerifyAssetRangeProof(AssetRangeProof assetRangeProof, byte[] assetCommitment)
        {
            GroupOperations.ge_frombytes(out GroupElementP3 assetCommitmentP3, assetCommitment, 0);
            GroupElementP3[] candidateAssetCommitmentsP3 = TranslatePoints(assetRangeProof.H);

            byte[] msg = CalcAssetRangeProofMsg(assetCommitmentP3, candidateAssetCommitmentsP3);

            GroupElementP3[] pubkeys = CalcAssetRangeProofPubkeys(assetCommitmentP3, candidateAssetCommitmentsP3);

            bool res = VerifyRingSignature(assetRangeProof.Rs, msg, pubkeys);

            return res;
        }

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
        internal static RingSignature CreateRingSignature(byte[] msg, GroupElementP3[] pks, int j, byte[] sk)
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

            if (pks.Length == 0)
            {
                ringSignature = new RingSignature();
                return ringSignature;
            }

            ulong n = (ulong)pks.Length;
            ringSignature = new RingSignature((int)n);

            // 1. Let `counter = 0`.
            ulong counter = 0;
            while (true)
            {
                byte[][] e0 = new byte[2][]; // second slot is to put non-zero value in a time-constant manner

                // 2. Calculate a sequence of: `n-1` 32-byte random values, 64-byte `nonce` and 1-byte `mask`:
                //    `{r[i], nonce, mask} = SHAKE256(counter || p || msg, 8*(32*(n-1) + 64 + 1))`,
                //    where `p` is encoded in 32 bytes using little-endian convention, and `counter` is encoded as a 64-bit little-endian integer.
                byte[][] r = new byte[n][];

                for (int m = 0; m < (int)n - 1; m++)
                {
                    r[m] = CryptoHelper.GetRandomSeed();
                }

                byte[] nonce = new byte[32];
                byte[] mask = new byte[1];

                // 3. Calculate `k = nonce mod L`, where `nonce` is interpreted as a 64-byte little-endian integer and reduced modulo subgroup order `L`.
                //byte[] k = ReduceScalar64(nonce);
                nonce = CryptoHelper.GetRandomSeed();
                ScalarOperations.sc_reduce32(nonce);
                byte[] k = nonce;

                // 4. Calculate the initial e-value, let `i = j+1 mod n`:
                ulong i = ((ulong)j + 1L) % n;

                // 4.1. Calculate `R[i]` as the point `k*G`.
                GroupOperations.ge_scalarmult_base(out GroupElementP3 Ri, k, 0);

                // 4.3. Calculate `e[i] = SHA3-512(R[i] || msg || i)` where `i` is encoded as a 64-bit little-endian integer. Interpret `e[i]` as a little-endian integer reduced modulo `L`.
                byte[] Rienc = new byte[32];
                GroupOperations.ge_p3_tobytes(Rienc, 0, ref Ri);

                byte[] ei = ComputeE(Rienc, msg, i);

                if (i == 0)
                {
                    e0[0] = new byte[32];
                    Array.Copy(ei, 0, e0[0], 0, ei.Length);
                }
                else
                {
                    e0[1] = new byte[32];
                    Array.Copy(ei, 0, e0[1], 0, ei.Length);
                }

                // 5. For `step` from `1` to `n-1` (these steps are skipped if `n` equals 1):
                for (ulong step = 1; step < n; step++)
                {
                    // 5.1. Let `i = (j + step) mod n`.
                    i = ((ulong)j + step) % n;

                    // 5.2. Set the forged s-value `s[i] = r[step-1]`
                    ringSignature.S[i] = new byte[32];
                    Array.Copy(r[step - 1], 0, ringSignature.S[i], 0, 32);

                    // 5.3. Define `z[i]` as `s[i]` with the most significant 4 bits set to zero.
                    byte[] z = new byte[32];
                    Array.Copy(ringSignature.S[i], 0, z, 0, 32);
                    z[31] &= 0x0f;

                    // 5.4. Define `w[i]` as a most significant byte of `s[i]` with lower 4 bits set to zero: `w[i] = s[i][31] & 0xf0`.
                    byte wi = (byte)(ringSignature.S[i][31] & 0xf0);

                    // 5.5. Let `i’ = i+1 mod n`.
                    ulong i1 = (i + 1) % n;

                    // 5.6. Calculate `R[i’] = z[i]*G - e[i]*P[i]` and encode it as a 32-byte public key.

                    byte[] nei = NegateScalar(ei);
                    GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, nei, ref pks[i], z);
                    byte[] Ri1 = new byte[32];
                    GroupOperations.ge_tobytes(Ri1, 0, ref p2);

                    // 5.7. Calculate `e[i’] = SHA3-512(R[i’] || msg || i’)` where `i’` is encoded as a 64-bit little-endian integer.
                    // Interpret `e[i’]` as a little-endian integer.
                    ei = ComputeE(Ri1, msg, i1);

                    if (i1 == 0)
                    {
                        e0[0] = new byte[32];
                        Array.Copy(ei, 0, e0[0], 0, ei.Length);
                    }
                    else
                    {
                        e0[1] = new byte[32];
                        Array.Copy(ei, 0, e0[1], 0, ei.Length);
                    }
                }

                // 6. Calculate the non-forged `z[j] = k + p*e[j] mod L` and encode it as a 32-byte little-endian integer.
                byte[] zj = new byte[32];
                ScalarOperations.sc_muladd(zj, sk, ei, k);

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
                    //zj[31] ^= (byte)(mask[0] & 0xf0); // zj now == sj

                    // Put non-forged s[j] into ringsig
                    Array.Copy(zj, 0, ringSignature.S[j], 0, zj.Length);

                    // Put e[0] inside the ringsig
                    Array.Copy(e0[0], 0, ringSignature.E, 0, e0[0].Length);

                    break;
                }
            }

            // 9. Return the ring signature `{e[0], s[0], ..., s[n-1]}`, total `n+1` 32-byte elements.
            return ringSignature;
        }

        internal static bool VerifyRingSignature(RingSignature ringSignature, byte[] msg, GroupElementP3[] pks)
        {
            if (ringSignature.S.Length != pks.Length)
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

                // 3. Calculate `R[i+1] = z[i]*G - e[i]*P[i]` and encode it as a 32-byte public key.
                byte[] R = new byte[32];
                byte[] ne = NegateScalar(e);

                GroupOperations.ge_double_scalarmult_vartime(out GroupElementP2 p2, ne, ref pks[i], z);
                GroupOperations.ge_tobytes(R, 0, ref p2);

                // 4. Calculate `e[i+1] = SHA3-512(R[i+1] || msg || i+1)` where `i+1` is encoded as a 64-bit little-endian integer.
                // 5. Interpret `e[i+1]` as a little-endian integer reduced modulo subgroup order `L`.
                e = ComputeE(R, msg, (ulong)((i + 1) % n));//, w);
            }

            return e.Equals32(ringSignature.E);
        }

        internal static byte[] CalcAssetRangeProofMsg(GroupElementP3 assetCommitment, GroupElementP3[] candidateAssetCommitments)
        {
            IHash hash = HashFactory.Crypto.CreateSHA256();
            hash.TransformBytes(assetCommitment.ToBytes());

            foreach (GroupElementP3 candidate in candidateAssetCommitments)
            {
                hash.TransformBytes(candidate.ToBytes());
            }

            byte[] msg = hash.TransformFinal().GetBytes();

            return msg;
        }
        // Calculate the set of public keys for the ring signature from the set of input asset ID commitments: `P[i] = H’ - H[i]`.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetCommitment"></param>
        /// <param name="candidateAssetCommitments"></param>
        /// <returns>array of 32 byte array representing point on EC</returns>
        internal static GroupElementP3[] CalcAssetRangeProofPubkeys(GroupElementP3 assetCommitment, GroupElementP3[] candidateAssetCommitments)
        {
            GroupElementP3[] pubKeys = new GroupElementP3[candidateAssetCommitments.Length];

            int index = 0;
            foreach (GroupElementP3 candidateAssetCommitment in candidateAssetCommitments)
            {
                GroupElementP3 candidateAssetCommitmentP3 = candidateAssetCommitment;
                GroupOperations.ge_p3_to_cached(out GroupElementCached candidateAssetCommitmentCached, ref candidateAssetCommitmentP3);
                GroupOperations.ge_sub(out GroupElementP1P1 pubKeyP1P1, ref assetCommitment, ref candidateAssetCommitmentCached);

                GroupOperations.ge_p1p1_to_p3(out GroupElementP3 pubKeyP3, ref pubKeyP1P1);
                pubKeys[index++] = pubKeyP3;
            }

            return pubKeys;
        }

        private static byte[] ComputeE(byte[] r, byte[] msg, ulong i)
        {
            byte[] hash = FastHash512(r, msg, BitConverter.GetBytes(i));
            byte[] res = ReduceScalar64(hash);

            return res;
        }

        private static byte[] FastHash512(params byte[][] bytes)
        {
            IHash hash = HashFactory.Crypto.SHA3.CreateKeccak512();
            return FastHash(bytes, hash);
        }

        private static byte[] FastHash256(params byte[][] bytes)
        {
            IHash hash = HashFactory.Crypto.CreateSHA256();
            return FastHash(bytes, hash);
        }

        private static byte[] FastHash(byte[][] bytes, IHash hash)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                hash.TransformBytes(bytes[i]);
            }
            byte[] hashValue = hash.TransformFinal().GetBytes();

            return hashValue;
        }

        public static byte[] ReduceScalar64(byte[] hash)
        {
            ScalarOperations.sc_reduce(hash);
            byte[] res = new byte[32];
            Array.Copy(hash, 0, res, 0, 32);
            return res;
        }

        private static byte[] NegateScalar(byte[] s)
        {
            byte[] res = new byte[32];
            ScalarOperations.sc_negate(res, s);

            return res;
        }

        private static GroupElementP3[] TranslatePoints(byte[][] points)
        {
            GroupElementP3[] pointsP3 = new GroupElementP3[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                GroupOperations.ge_frombytes(out pointsP3[i], points[i], 0);
            }

            return pointsP3;
        }
    }
}
