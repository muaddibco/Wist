using Chaos.NaCl;
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
            byte[] asset1Seed = GetRandomSeed();
            byte[] asset2Seed = GetRandomSeed();

            byte[] blinding1 = GetRandomSeed();
            byte[] blinding2 = GetRandomSeed();

            byte[] asset1 = Ed25519.PublicKeyFromSeed(asset1Seed);
            byte[] asset2 = Ed25519.PublicKeyFromSeed(asset2Seed);

            byte[] in1F = Ed25519.PublicKeyFromSeed(blinding1);
            byte[] out1F = Ed25519.PublicKeyFromSeed(blinding2);

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
