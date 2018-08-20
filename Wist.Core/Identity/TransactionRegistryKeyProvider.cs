using System;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Identity
{
    [RegisterExtension(typeof(IIdentityKeyProvider), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistryKeyProvider : IIdentityKeyProvider
    {
        public string Name => "TransactionRegistry";

        public IEqualityComparer<IKey> GetComparer() => new TransactionRegistryKey();

        public IKey GetKey(byte[] keyBytes)
        {
            if (keyBytes == null)
            {
                throw new ArgumentNullException(nameof(keyBytes));
            }

            if (keyBytes.Length != 16)
            {
                throw new ArgumentOutOfRangeException("The size of byte array must be 16 bytes");
            }

            return new TransactionRegistryKey { Value = keyBytes };
        }
    }
}
