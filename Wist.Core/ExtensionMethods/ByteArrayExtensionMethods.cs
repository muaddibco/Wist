using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;

namespace Wist.Core.ExtensionMethods
{
    public static class ByteArrayExtensionMethods
    {
        private static readonly uint[] _lookup32Unsafe = CreateLookup32Unsafe();
        private static readonly unsafe uint* _lookup32UnsafeP = (uint*)GCHandle.Alloc(_lookup32Unsafe, GCHandleType.Pinned).AddrOfPinnedObject();

        private static uint[] CreateLookup32Unsafe()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                if (BitConverter.IsLittleEndian)
                    result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
                else
                    result[i] = ((uint)s[1]) + ((uint)s[0] << 16);
            }
            return result;
        }

        public static unsafe string ToHexString(this byte[] arr)
        {
            if (arr != null)
            {
                var lookupP = _lookup32UnsafeP;
                var result = new char[arr.Length * 2];
                fixed (byte* bytesP = arr)
                fixed (char* resultP = result)
                {
                    uint* resultP2 = (uint*)resultP;
                    for (int i = 0; i < arr.Length; i++)
                    {
                        resultP2[i] = lookupP[bytesP[i]];
                    }
                }
                return new string(result);
            }
            return null;
        }
    }
}
