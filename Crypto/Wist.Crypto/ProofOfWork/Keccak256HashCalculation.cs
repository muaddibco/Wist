using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class Keccak256HashCalculation : IHashCalculation
    {
        public HashType HashType => HashType.Keccak256;

        public int HashSize => _hash.HashSize;

        private readonly IHash _hash;

        public Keccak256HashCalculation()
        {
            _hash = HashFactory.Crypto.SHA3.CreateKeccak256();
        }

        public byte[] CalculateHash(byte[] input)
        {
            HashResult hashRes = _hash.ComputeBytes(input);
            return hashRes.GetBytes();
        }
    }
}
