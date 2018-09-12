using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Node.Core.Registry
{
    [ServiceContract]
    public interface ITransactionsRegistryHelper
    {
        IKey GetTransactionRegistryHashKey(RegistryRegisterBlock registryRegisterBlock);

        IKey GetTransactionRegistryTwiceHashedKey(RegistryRegisterBlock registryRegisterBlock);
    }
}
