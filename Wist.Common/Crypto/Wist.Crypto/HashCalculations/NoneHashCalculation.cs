using System;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
    //[RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class NoneHashCalculation : IHashCalculation
    {
        public HashType HashType => HashType.None;

        public int HashSize => 0;

        public byte[] CalculateHash(Memory<byte> input) => null;

        public byte[] CalculateHash(byte[] input) => null;
    }
}
