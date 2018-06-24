using System;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Identity
{

    [RegisterExtension(typeof(IIdentityKeyProvider), Lifetime = LifetimeManagement.Singleton)]
    public class EcdsaPublicKeyProvider : IIdentityKeyProvider
    {
        public string Name => "Ecdsa";

        public IKey GetKey(byte[] keyBytes)
        {
            if (keyBytes == null)
            {
                throw new ArgumentNullException(nameof(keyBytes));
            }

            if(keyBytes.Length != 32)
            {
                throw new ArgumentOutOfRangeException("The size of byte array must be 32 bytes");
            }

            return new Public32Key { Value = keyBytes };
        }
    }
}
