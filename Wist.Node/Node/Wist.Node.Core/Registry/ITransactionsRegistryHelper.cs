using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Node.Core.Registry
{
    [ServiceContract]
    public interface ITransactionsRegistryHelper
    {
        IKey GetTransactionRegistryHashKey(ITransactionRegistryBlock registryRegisterBlock);

        IKey GetTransactionRegistryTwiceHashedKey(ITransactionRegistryBlock registryRegisterBlock);
    }
}
