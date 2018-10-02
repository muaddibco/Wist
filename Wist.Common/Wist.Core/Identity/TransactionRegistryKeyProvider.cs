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

        public IEqualityComparer<IKey> GetComparer() => new Key16();

        public IKey GetKey(Memory<byte> keyBytes)
        {
            if (keyBytes.Length != 16)
            {
                throw new ArgumentOutOfRangeException("The size of byte array must be 16 bytes");
            }

            return new Key16(keyBytes);
        }
    }
}
