using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Wist.Core.Exceptions;

namespace Wist.Core.ExtensionMethods
{
    public static class MemoryExtensionMethods
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

        public static ArraySegment<byte> ToArraySegment(this Memory<byte> arr)
        {
            if (!MemoryMarshal.TryGetArray(arr, out ArraySegment<byte> byteArray))
            {
                throw new FailedToMarshalToByteArrayException(nameof(arr));
            }

            return byteArray;
        }

        public static unsafe string ToHexString(this Memory<byte> arr, int offset = 0, int length = 0)
        {
            if (!MemoryMarshal.TryGetArray(arr, out ArraySegment<byte> byteArray))
            {
                throw new FailedToMarshalToByteArrayException(nameof(arr));
            }

            var arrLength = (length == 0 ? arr.Length : length);
            var lookupP = _lookup32UnsafeP;
            var result = new char[arrLength * 2];
            fixed (byte* bytesP = byteArray.Array)
            {
                byte* bytesP1 = bytesP + byteArray.Offset;
                fixed (char* resultP = result)
                {
                    uint* resultP2 = (uint*)resultP;
                    for (int i = 0; i < arrLength; i++)
                    {
                        resultP2[i] = lookupP[bytesP1[i + offset]];
                    }
                }
            }
            return new string(result);
        }

        /// <summary>
        /// Checks equality of hash values. Function checks only hash values with sizes multiple of 16.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe bool EqualsX16(this Memory<byte> a, Memory<byte> b)
        {
            int length = a.Length;
            if (length != b.Length)
            {
                return false;
            }

            if (!MemoryMarshal.TryGetArray(a, out ArraySegment<byte> byteArrayA))
            {
                throw new FailedToMarshalToByteArrayException(nameof(a));
            }

            if (!MemoryMarshal.TryGetArray(b, out ArraySegment<byte> byteArrayB))
            {
                throw new FailedToMarshalToByteArrayException(nameof(b));
            }

            if (byteArrayA.Count % 16 != 0)
                throw new ArgumentOutOfRangeException("EqualsX16 can check only sizes of hash values multiple of 16");

            fixed (byte* a1 = byteArrayA.Array)
            {
                byte* pa1 = a1 + byteArrayA.Offset;
                fixed (byte* b1 = byteArrayB.Array)
                {
                    byte* pb1 = b1 + byteArrayB.Offset;
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
        public static unsafe bool Equals24(this Memory<byte> a, Memory<byte> b)
        {
            if (a.Length != 24 || b.Length != 24)
                throw new ArgumentOutOfRangeException("Equals24 can check only sizes of hash values multiple of 24");

            if (!MemoryMarshal.TryGetArray(a, out ArraySegment<byte> byteArrayA))
            {
                throw new FailedToMarshalToByteArrayException(nameof(a));
            }

            if (!MemoryMarshal.TryGetArray(b, out ArraySegment<byte> byteArrayB))
            {
                throw new FailedToMarshalToByteArrayException(nameof(b));
            }

            fixed (byte* a1 = byteArrayA.Array)
            {
                byte* pa1 = a1 + byteArrayA.Offset;
                fixed (byte* b1 = byteArrayB.Array)
                {
                    byte* pb1 = b1 + byteArrayB.Offset;
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
        public static unsafe bool Equals32(this Memory<byte> a, Memory<byte> b)
        {
            if (a.Length != 32 || b.Length != 32)
                throw new ArgumentOutOfRangeException($"{nameof(Equals32)} can check only sizes of hash values of 32 bytes");

            if (!MemoryMarshal.TryGetArray(a, out ArraySegment<byte> byteArrayA))
            {
                throw new FailedToMarshalToByteArrayException(nameof(a));
            }

            if (!MemoryMarshal.TryGetArray(b, out ArraySegment<byte> byteArrayB))
            {
                throw new FailedToMarshalToByteArrayException(nameof(b));
            }

            fixed (byte* a1 = byteArrayA.Array)
            {
                byte* pa1 = a1 + byteArrayA.Offset;
                fixed (byte* b1 = byteArrayB.Array)
                {
                    byte* pb1 = b1 + byteArrayB.Offset;
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
        public static unsafe bool Equals64(this Memory<byte> a, Memory<byte> b)
        {
            if (a.Length != 64 || b.Length != 64)
                throw new ArgumentOutOfRangeException($"{nameof(Equals64)} can check only sizes of hash values of 64 bytes");

            if (!MemoryMarshal.TryGetArray(a, out ArraySegment<byte> byteArrayA))
            {
                throw new FailedToMarshalToByteArrayException(nameof(a));
            }

            if (!MemoryMarshal.TryGetArray(b, out ArraySegment<byte> byteArrayB))
            {
                throw new FailedToMarshalToByteArrayException(nameof(b));
            }

            fixed (byte* a1 = byteArrayA.Array)
            {
                byte* pa1 = a1 + byteArrayA.Offset;
                fixed (byte* b1 = byteArrayB.Array)
                {
                    byte* pb1 = b1 + byteArrayB.Offset;
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
        public static unsafe int GetHashCode16(this Memory<byte> data)
        {
            if (data.Length != 16)
                throw new ArgumentOutOfRangeException($"{nameof(GetHashCode16)} can work with byte arrays of length of 16 bytes only");

            if (!MemoryMarshal.TryGetArray(data, out ArraySegment<byte> byteArray))
            {
                throw new FailedToMarshalToByteArrayException(nameof(data));
            }

            fixed (byte* a = byteArray.Array)
            {
                byte* a_1 = a + byteArray.Offset;
                unchecked
                {
                    const int p = 16777619;
                    int hash = (int)2166136261;

                    int* a1 = (int*)a_1;
                    int* a2 = (int*)(a_1 + 4);
                    int* a3 = (int*)(a_1 + 8);
                    int* a4 = (int*)(a_1 + 12);

                    hash = (hash ^ *a1) * p;
                    hash = (hash ^ *a2) * p;
                    hash = (hash ^ *a3) * p;
                    hash = (hash ^ *a4) * p;

                    hash += hash << 13;
                    hash ^= hash >> 7;
                    hash += hash << 3;
                    hash ^= hash >> 17;
                    hash += hash << 5;
                    return hash;
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static unsafe int GetHashCode32(this Memory<byte> data)
        {
            if (data.Length != 32)
                throw new ArgumentOutOfRangeException($"{nameof(GetHashCode32)} can work with byte arrays of length of 32 bytes only");

            if (!MemoryMarshal.TryGetArray(data, out ArraySegment<byte> byteArray))
            {
                throw new FailedToMarshalToByteArrayException(nameof(data));
            }

            fixed (byte* a = byteArray.Array)
            {
                byte* a_1 = a + byteArray.Offset;
                unchecked
                {
                    const int p = 16777619;
                    int hash = (int)2166136261;

                    int* a1 = (int*)a_1;
                    int* a2 = (int*)(a_1 + 4);
                    int* a3 = (int*)(a_1 + 8);
                    int* a4 = (int*)(a_1 + 12);
                    int* a5 = (int*)(a_1 + 16);
                    int* a6 = (int*)(a_1 + 20);
                    int* a7 = (int*)(a_1 + 24);
                    int* a8 = (int*)(a_1 + 28);

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
