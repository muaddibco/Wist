using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class Keccak256HashCalculation : HashCalculationBase
    {
        public override HashType HashType => HashType.Keccak256;

        public Keccak256HashCalculation()
            : base(HashFactory.Crypto.SHA3.CreateKeccak256())
        {
        }
    }
}
