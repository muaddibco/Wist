using System;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Identity
{

    [RegisterExtension(typeof(IIdentityKeyProvider), Lifetime = LifetimeManagement.Singleton)]
    public class DefaultKeyProvider : IIdentityKeyProvider
    {
        public string Name => "Default";

        public IEqualityComparer<IKey> GetComparer() => new Key32();

        public IKey GetKey(Memory<byte> keyBytes)
        {
            if(keyBytes.Length != 32)
            {
                throw new ArgumentOutOfRangeException("The size of byte array must be 32 bytes");
            }

            return new Key32(keyBytes);
        }
    }
}
