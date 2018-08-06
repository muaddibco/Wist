using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ProofOfWork;

namespace Wist.Crypto.ProofOfWork
{
    [RegisterExtension(typeof(IProofOfWorkCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class MurMurProofOfWorkCalculation : IProofOfWorkCalculation
    {
        public POWType POWType => POWType.MurMur;

        public int HashSize => _hash.HashSize;

        private readonly IHash _hash;

        public MurMurProofOfWorkCalculation()
        {
            _hash = HashFactory.Hash128.CreateMurmur3_128();
        }

        public byte[] CalculateHash(byte[] input)
        {
            HashResult hashRes = _hash.ComputeBytes(input);
            return hashRes.GetBytes();
        }
    }
}
