using Chaos.NaCl;
using System;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;

namespace Wist.Crypto
{
    [RegisterDefaultImplementation(typeof(ICryptoService), Lifetime = LifetimeManagement.Singleton)]
    public class CryptoServiceEd25519 : ICryptoService
    {
        private byte[] _expandedPrivateKey;
        private readonly IHashCalculation _hashCalculation;

        public CryptoServiceEd25519(IHashCalculationRepository hashCalculationRepository)
        {
            _hashCalculation = hashCalculationRepository.Create(HashType.MurMur);
        }

        public byte[] ComputeTransactionKey(byte[] bytes) => _hashCalculation.CalculateHash(bytes);

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
