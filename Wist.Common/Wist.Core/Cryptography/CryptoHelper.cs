using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;

namespace Wist.Core.Cryptography
{
    public static class CryptoHelper
    {
        /// <summary>
        /// Calculates SHA512 HASH
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ComputeHash(byte[] input)
        {
            //using (SHA512 hashAlgorithm = SHA512CryptoServiceProvider.Create())
            using (SHA256 hashAlgorithm = SHA256CryptoServiceProvider.Create())
            {
                byte[] hash = hashAlgorithm.ComputeHash(input);
                return hash;
            }            
        }

        public static byte[] ComputeHash(byte[] input, uint level)
        {
            byte[] buffer = input;
            for (int i = 0; i < level; i++)
            {
                //using (SHA512 hashAlgorithm = SHA512CryptoServiceProvider.Create())
                using (SHA256 hashAlgorithm = SHA256CryptoServiceProvider.Create())
                {
                    buffer = hashAlgorithm.ComputeHash(buffer);
                }
            }
            return buffer;
        }

        public static byte[] GetRandomSeed()
        {
            byte[] seed = new byte[32];
            RNGCryptoServiceProvider.Create().GetNonZeroBytes(seed);

            return seed;
        }
    }
}