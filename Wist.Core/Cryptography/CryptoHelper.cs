using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;

namespace Wist.Core.Cryptography
{
    public static class CryptoHelper
    {
        public static byte[] ComputeHash(byte[] input)
        {
            using (SHA512 sha512 = SHA512CryptoServiceProvider.Create())
            {
                byte[] hash = sha512.ComputeHash(input);
                return hash;
            }            
        }

        public static byte[] ComputeHash(byte[] input, uint level)
        {
            {
                byte[] buffer = input;
                for (int i = 0; i < level; i++)
                {
                    using (SHA512 sha512 = SHA512CryptoServiceProvider.Create())
                        buffer = sha512.ComputeHash(buffer);
                }
                return buffer;
            }
        }

        /// <summary>
        /// Checks equality of hash values. Function checks only hash values with sizes multiple of 16.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool Hash64Equals(byte[] a, byte[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (a.Length % 16 != 0 || b.Length % 16 != 0)
                throw new ArgumentOutOfRangeException("Hash64Equals can check only sizes of hash values multiple of 16");

            int length = a.Length;
            if (length != b.Length)
            {
                return false;
            }

            fixed (byte* a1 = a)
            {
                byte* pa1 = a1;
                fixed (byte* b1 = b)
                {
                    byte* pb1 = b1;
                    byte* pa1_1 = pa1;
                    byte* pb1_1 = pb1;
                    while (length >= 0)
                    {
                        if ((((*(((int*)pa1_1)) != *(((int*)pb1_1))) || (*(((int*)(pa1_1 + 4))) != *(((int*)(pb1_1 + 4))))) || ((*(((int*)(pa1_1 + 8))) != *(((int*)(pb1_1 + 8)))) || (*(((int*)(pa1_1 + 12))) != *(((int*)(pb1_1 + 12)))))))
                        {
                            break;
                        }
                        pa1_1 += 16;
                        pb1_1 += 16;
                        length -= 16;
                    }
                    return (length <= 0);
                }
            }
        }
    }
}