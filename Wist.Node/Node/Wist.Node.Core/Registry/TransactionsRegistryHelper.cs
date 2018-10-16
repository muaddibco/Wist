using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;

namespace Wist.Node.Core.Registry
{
    [RegisterDefaultImplementation(typeof(ITransactionsRegistryHelper), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryHelper : ITransactionsRegistryHelper
    {
        private readonly ICryptoService _cryptoService;
        private readonly IIdentityKeyProvider _transactionHashKey;

        public TransactionsRegistryHelper(ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _cryptoService = cryptoService;
            _transactionHashKey = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
        }

        public IKey GetTransactionRegistryHashKey(ITransactionRegistryBlock registryRegisterBlock)
        {
            IKey key = _transactionHashKey.GetKey(_cryptoService.ComputeTransactionKey(registryRegisterBlock.RawData));

            return key;
        }

        public IKey GetTransactionRegistryTwiceHashedKey(ITransactionRegistryBlock registryRegisterBlock)
        {
            byte[] hash = _cryptoService.ComputeTransactionKey(_cryptoService.ComputeTransactionKey(registryRegisterBlock.RawData));
            IKey key = _transactionHashKey.GetKey(hash);

            return key;
        }
    }
}
