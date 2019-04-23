using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class MurMurHashCalculation : HashCalculationBase
    {
        public override HashType HashType => HashType.MurMur;

        public MurMurHashCalculation() 
            : base(HashFactory.Hash128.CreateMurmur3_128())
        {
        }
    }
}
