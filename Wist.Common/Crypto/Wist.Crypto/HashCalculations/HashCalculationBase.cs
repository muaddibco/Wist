using HashLib;
using System;
using System.Runtime.InteropServices;
using Wist.Core.Exceptions;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
    public abstract class HashCalculationBase : IHashCalculation
    {
        protected readonly IHash _hash;

        public abstract HashType HashType { get; }

        public virtual int HashSize => _hash.HashSize;

        public HashCalculationBase(IHash hash)
        {
            _hash = hash;
        }

        public byte[] CalculateHash(byte[] input)
        {
            lock (_hash)
            {
                HashResult hashRes = _hash.ComputeBytes(input);
                return hashRes.GetBytes();
            }
        }

        public byte[] CalculateHash(Memory<byte> input)
        {
            lock (_hash)
            {
                if (MemoryMarshal.TryGetArray(input, out ArraySegment<byte> byteArray))
                {
                    HashResult hashRes = _hash.ComputeBytes(byteArray.Array);
                    return hashRes.GetBytes();
                }

                throw new FailedToMarshalToByteArrayException(nameof(input));
            }
        }
    }
}
