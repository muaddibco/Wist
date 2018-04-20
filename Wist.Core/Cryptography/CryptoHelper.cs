using System;
using System.Collections.Generic;
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
    }
}