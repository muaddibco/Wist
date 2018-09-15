using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class MurMurHashCalculation : IHashCalculation
    {
        public HashType HashType => HashType.MurMur;

        public int HashSize => _hash.HashSize;

        private readonly IHash _hash;

        public MurMurHashCalculation()
        {
            _hash = HashFactory.Hash128.CreateMurmur3_128();
        }

        public byte[] CalculateHash(byte[] input)
        {
            lock (_hash)
            {
                HashResult hashRes = _hash.ComputeBytes(input);
                return hashRes.GetBytes();
            }
        }
    }
}
