﻿using System;
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

        /// <summary>
        /// Checks equality of hash values. Function checks only hash values with sizes multiple of 16.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool EqualsX16(byte[] a, byte[] b)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));

            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (a.Length % 16 != 0 || b.Length % 16 != 0)
                throw new ArgumentOutOfRangeException("HashX16Equals can check only sizes of hash values multiple of 16");

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
    }
}
