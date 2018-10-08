using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Wist.Core.Exceptions;

namespace Wist.Core.Cryptography
{
    public static class CryptoHelper
    {
        /// <summary>
        /// Calculates SHA256 HASH
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

        /// <summary>
        /// Calculates SHA256 HASH
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ComputeHash(Memory<byte> input)
        {
            if (MemoryMarshal.TryGetArray(input, out ArraySegment<byte> byteArray))
            {
                //using (SHA512 hashAlgorithm = SHA512CryptoServiceProvider.Create())
                using (SHA256 hashAlgorithm = SHA256CryptoServiceProvider.Create())
                {
                    byte[] hash = hashAlgorithm.ComputeHash(byteArray.Array);
                    return hash;
                }
            }

            throw new FailedToMarshalToByteArrayException(nameof(input));
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

        public static byte[] GetScalar(ulong value)
        {
            byte[] scalar = new byte[32];

            BitConverter.GetBytes(value).CopyTo(scalar, 0);

            return scalar;
        }
    }
}