using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Serializers
{
    public interface ITransactionKeyProvider
    {
        IKey GetKey();
    }
}
