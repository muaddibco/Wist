using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ProofOfWork;

namespace Wist.Crypto.ProofOfWork
{
    [RegisterExtension(typeof(IProofOfWorkCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class Tiger4ProofOfWorkCalculation : IProofOfWorkCalculation
    {
        public POWType POWType => POWType.Tiger4;

        public int HashSize => _hash.HashSize;

        private readonly IHash _hash;

        public Tiger4ProofOfWorkCalculation()
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
