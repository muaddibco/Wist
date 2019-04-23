using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
    [RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class Tiger4HashCalculation : HashCalculationBase
    {
        public override HashType HashType => HashType.Tiger4;

        public Tiger4HashCalculation()
            : base(HashFactory.Crypto.CreateTiger_4_192())
        {
        }
    }
}
