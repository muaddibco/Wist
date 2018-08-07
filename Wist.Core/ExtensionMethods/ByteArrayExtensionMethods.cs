using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

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

        public static unsafe string ToHexString(this byte[] arr, int offset = 0, int length = 0)
        {
            if (arr != null)
            {
                var arrLength = (length == 0 ? arr.Length : length);
                var lookupP = _lookup32UnsafeP;
                var result = new char[arrLength * 2];
                fixed (byte* bytesP = arr)
                fixed (char* resultP = result)
                {
                    uint* resultP2 = (uint*)resultP;
                    for (int i = 0; i < arrLength; i++)
                    {
                        resultP2[i] = lookupP[bytesP[i + offset]];
                    }
                }
                return new string(result);
            }
            return null;
        }

        /// <summary>
        /// Checks equality of hash values. Function checks only hash values with sizes multiple of 16.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool EqualsX16(this byte[] a, byte[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (a.Length % 16 != 0 || b.Length % 16 != 0)
                throw new ArgumentOutOfRangeException("EqualsX16 can check only sizes of hash values multiple of 16");

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

        /// <summary>
        /// Checks equality of hash values. Function checks only hash values with sizes multiple of 16.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool Equals24(this byte[] a, byte[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (a.Length != 24 || b.Length != 24)
                throw new ArgumentOutOfRangeException("Equals24 can check only sizes of hash values multiple of 24");

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
                    long* pla1 = (long*)pa1;
                    long* pla2 = (long*)(pa1 + 8);
                    long* pla3 = (long*)(pa1 + 16);
                    long* plb1 = (long*)pb1;
                    long* plb2 = (long*)(pb1 + 8);
                    long* plb3 = (long*)(pb1 + 16);

                    return *pla1 == *plb1
                        && *pla2 == *plb2
                        && *pla3 == *plb3;
                }
            }
        }

        /// <summary>
        /// Checks equality of hash values. Function checks only hash values with sizes of 32.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool Equals32(this byte[] a, byte[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (a.Length != 32 || b.Length != 32)
                throw new ArgumentOutOfRangeException("Hash64Equals can check only sizes of hash values of 32 bytes");

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
                    long* pla1 = (long*)pa1;
                    long* pla2 = (long*)(pa1 + 8);
                    long* pla3 = (long*)(pa1 + 16);
                    long* pla4 = (long*)(pa1 + 24);
                    long* plb1 = (long*)pb1;
                    long* plb2 = (long*)(pb1 + 8);
                    long* plb3 = (long*)(pb1 + 16);
                    long* plb4 = (long*)(pb1 + 24);

                    return *pla1 == *plb1
                        && *pla2 == *plb2
                        && *pla3 == *plb3
                        && *pla4 == *plb4;
                }
            }
        }

        /// <summary>
        /// Checks equality of hash values. Function checks only hash values with sizes of 64.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool Equals64(this byte[] a, byte[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (a.Length != 64 || b.Length != 64)
                throw new ArgumentOutOfRangeException("Hash64Equals can check only sizes of hash values of 64 bytes");

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
                    long* pla1 = (long*)pa1;
                    long* pla2 = (long*)(pa1 + 8);
                    long* pla3 = (long*)(pa1 + 16);
                    long* pla4 = (long*)(pa1 + 24);
                    long* pla5 = (long*)(pa1 + 32);
                    long* pla6 = (long*)(pa1 + 40);
                    long* pla7 = (long*)(pa1 + 48);
                    long* pla8 = (long*)(pa1 + 56);
                    long* plb1 = (long*)pb1;
                    long* plb2 = (long*)(pb1 + 8);
                    long* plb3 = (long*)(pb1 + 16);
                    long* plb4 = (long*)(pb1 + 24);
                    long* plb5 = (long*)(pb1 + 32);
                    long* plb6 = (long*)(pb1 + 40);
                    long* plb7 = (long*)(pb1 + 48);
                    long* plb8 = (long*)(pb1 + 56);

                    return *pla1 == *plb1
                        && *pla2 == *plb2
                        && *pla3 == *plb3
                        && *pla4 == *plb4
                        && *pla5 == *plb5
                        && *pla6 == *plb6
                        && *pla7 == *plb7
                        && *pla8 == *plb8;
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe int GetHashCode32(this byte[] data)
        {
            if (data.Length != 32)
                throw new ArgumentOutOfRangeException("GetHashCode32 can work with byte arrays of length of 32 bytes only");

            fixed (byte* a = data)
            {
                unchecked
                {
                    const int p = 16777619;
                    int hash = (int)2166136261;

                    int* a1 = (int*)a;
                    int* a2 = (int*)(a + 4);
                    int* a3 = (int*)(a + 8);
                    int* a4 = (int*)(a + 12);
                    int* a5 = (int*)(a + 16);
                    int* a6 = (int*)(a + 20);
                    int* a7 = (int*)(a + 24);
                    int* a8 = (int*)(a + 28);

                    hash = (hash ^ *a1) * p;
                    hash = (hash ^ *a2) * p;
                    hash = (hash ^ *a3) * p;
                    hash = (hash ^ *a4) * p;
                    hash = (hash ^ *a5) * p;
                    hash = (hash ^ *a6) * p;
                    hash = (hash ^ *a7) * p;
                    hash = (hash ^ *a8) * p;

                    hash += hash << 13;
                    hash ^= hash >> 7;
                    hash += hash << 3;
                    hash ^= hash >> 17;
                    hash += hash << 5;
                    return hash;
                }
            }
        }
    }
}
