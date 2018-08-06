using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ProofOfWork;

namespace Wist.Crypto.ProofOfWork
{
    [RegisterExtension(typeof(IProofOfWorkCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class NoneProofOfWorkCalculation : IProofOfWorkCalculation
    {
        public POWType POWType => POWType.None;

        public int HashSize => 0;

        public byte[] CalculateHash(byte[] input)
        {
            return null;
        }
    }
}
