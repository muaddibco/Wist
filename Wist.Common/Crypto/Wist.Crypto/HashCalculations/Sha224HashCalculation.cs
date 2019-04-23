using HashLib;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.Crypto.HashCalculations
{
	[RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
	public class Sha224HashCalculation : HashCalculationBase
	{
		public override HashType HashType => HashType.Sha224;

		public Sha224HashCalculation()
			: base(HashFactory.Crypto.CreateSHA224())
		{
		}
	}
}
