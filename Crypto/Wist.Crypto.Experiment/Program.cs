using Chaos.NaCl;
using Chaos.NaCl.Internal.Ed25519Ref10;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Wist.Crypto.Experiment
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] bytes1 = BitConverter.GetBytes(1);
            byte[] bytes2 = BitConverter.GetBytes(2);
            byte[] asset1Seed =  GetRandomSeed();
            byte[] asset2Seed = GetRandomSeed();

            byte[] blinding1Seed = new byte[32];// GetRandomSeed();
            byte[] blinding2Seed = new byte[32];// GetRandomSeed();
            Array.Copy(bytes1, 0, blinding1Seed, 0, bytes1.Length);
            Array.Copy(bytes2, 0, blinding2Seed, 0, bytes1.Length);

            GroupElementP3 in1F_P3;
            GroupElementP3 out1F_P3;
            GroupOperations.ge_scalarmult_base(out in1F_P3, blinding1Seed, 0);
            GroupOperations.ge_scalarmult_base(out out1F_P3, blinding2Seed, 0);

            GroupElementCached in1F_Cached;
            GroupOperations.ge_p3_to_cached(out in1F_Cached, ref in1F_P3);

            GroupElementP1P1 out1_in1_Diff_P1P1;
            GroupOperations.ge_sub(out out1_in1_Diff_P1P1, ref out1F_P3, ref in1F_Cached);

            GroupElementP2 out1_in1_diff_P2;
            GroupOperations.ge_p1p1_to_p2(out out1_in1_diff_P2, ref out1_in1_Diff_P1P1);
            byte[] out1_in1_diff_P2_bytes = new byte[64];
            GroupOperations.ge_tobytes(out1_in1_diff_P2_bytes, 0, ref out1_in1_diff_P2);


            BigInteger blinding1Int_ = new BigInteger(blinding1Seed);
            BigInteger blinding2Int_ = new BigInteger(blinding2Seed);
            BigInteger blindingsDiff_ = blinding2Int_ - blinding1Int_;
            byte[] blindingsSeedDiff = blindingsDiff_.ToByteArray();
            byte[] blindingsSeedDiff32 = new byte[32];
            Array.Copy(blindingsSeedDiff, 0, blindingsSeedDiff32, 0, Math.Min(blindingsSeedDiff.Length, blindingsSeedDiff32.Length));
            GroupElementP3 blindingsSeedDiff_P3;
            GroupOperations.ge_scalarmult_base(out blindingsSeedDiff_P3, blindingsSeedDiff32, 0);
            byte[] blindingsSeedDiff_P3_bytes = new byte[64];
            GroupOperations.ge_p3_tobytes(blindingsSeedDiff_P3_bytes, 0, ref blindingsSeedDiff_P3);





            byte[] asset1Code;
            byte[] asset2Code;

            byte[] asset1;
            byte[] asset2;

            Ed25519.KeyPairFromSeed(out asset1, out asset1Code, asset1Seed);
            Ed25519.KeyPairFromSeed(out asset2, out asset2Code, asset2Seed);

            byte[] blinding1;
            byte[] blinding2;

            byte[] in1F;
            byte[] out1F;

            Ed25519.KeyPairFromSeed(out in1F, out blinding1, blinding1Seed);
            Ed25519.KeyPairFromSeed(out out1F, out blinding2, blinding2Seed);

            BigInteger in1 = new BigInteger(in1F);
            BigInteger A1 = new BigInteger(asset1);

            in1 += A1;

            byte[] in1bytes = in1.ToByteArray();

            BigInteger out1 = new BigInteger(out1F);
            out1 += A1;

            byte[] out1bytes = out1.ToByteArray();

            BigInteger out1MinusIn1 = out1 - in1;
            byte[] diffBytes = out1MinusIn1.ToByteArray();

            BigInteger blinding1Int = new BigInteger(blinding1);
            BigInteger blinding2Int = new BigInteger(blinding2);

            BigInteger blindingDiff = blinding2Int - blinding1Int;

            byte[] blindingDiffBytes = blindingDiff.ToByteArray();
            byte[] diff1 = Ed25519.PublicKeyFromSeed(blindingDiffBytes);

            byte[] assetDenom = GetRandomSeed();
        }

        private static  byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }
    }
}
