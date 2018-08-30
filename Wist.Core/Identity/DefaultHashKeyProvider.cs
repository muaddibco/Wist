﻿using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Core.Identity
{
    [RegisterExtension(typeof(IIdentityKeyProvider), Lifetime = LifetimeManagement.Singleton)]
    public class DefaultHashKeyProvider : IIdentityKeyProvider
    {
        public string Name => "DefaultHash";

        public IEqualityComparer<IKey> GetComparer() => new Key32();

        public IKey GetKey(byte[] keyBytes)
        {
            if (keyBytes == null)
            {
                throw new ArgumentNullException(nameof(keyBytes));
            }

            if (keyBytes.Length != 32)
            {
                throw new ArgumentOutOfRangeException("The size of byte array must be 32 bytes");
            }

            return new Key32 { Value = keyBytes };
        }
    }
}
