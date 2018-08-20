using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class Tiger4HashCalculation : IHashCalculation
    {
        public HashType HashType => HashType.Tiger4;

        public int HashSize => _hash.HashSize;

        private readonly IHash _hash;

        public Tiger4HashCalculation()
        {
            _hash = HashFactory.Crypto.CreateTiger_4_192();
        }

        public byte[] CalculateHash(byte[] input)
        {
            HashResult hashRes = _hash.ComputeBytes(input);
            return hashRes.GetBytes();
        }
    }
}
