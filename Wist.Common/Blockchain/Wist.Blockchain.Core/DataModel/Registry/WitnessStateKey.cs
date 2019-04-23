using Wist.Core.Identity;

namespace Wist.Blockchain.Core.DataModel.Registry
{
    public class WitnessStateKey
    {
        public IKey PublicKey { get; set; }

        public ulong Height { get; set; }
    }
}
