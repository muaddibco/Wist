using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;

namespace Wist.Crypto
{
    [RegisterDefaultImplementation(typeof(ICryptoService), Lifetime = LifetimeManagement.Singleton)]
    public class CryptoServiceEd25519 : ICryptoService
    {
        private byte[] _expandedPrivateKey;

        public CryptoServiceEd25519()
        {

        }

        public void Initialize(byte[] privateKey)
        {
            if (privateKey == null)
            {
                throw new ArgumentNullException(nameof(privateKey));
            }

            _expandedPrivateKey = Ed25519.ExpandedPrivateKeyFromSeed(privateKey);
        }

        public byte[] Sign(byte[] message)
        {
            return Ed25519.Sign(message, _expandedPrivateKey);
        }

        public bool Verify(byte[] signature, byte[] message, byte[] publickKey)
        {
            return Ed25519.Verify(signature, message, publickKey);
        }
    }

}
