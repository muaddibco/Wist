using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Core.ExtensionMethods
{
    public static class UlongExtensionMethods
    {
        public static byte[] ToByteArray(this ulong i, int outputLength = 32)
        {
            if(outputLength < sizeof(ulong))
            {
                throw new ArgumentOutOfRangeException(nameof(outputLength), $"length of output array must be greater than {sizeof(ulong)} bytes");
            }
            byte[] valueBytes = BitConverter.GetBytes(i);
            byte[] res = new byte[outputLength];

            Array.Copy(valueBytes, 0, res, 0, valueBytes.Length);

            return res;
        }
    }
}
