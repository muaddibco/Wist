using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Serializers
{
    public interface ITransactionKeyProvider
    {
        IKey GetKey();
    }
}
