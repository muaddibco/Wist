using Chaos.NaCl;
using System;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Models;
using Wist.Crypto.Exceptions;
using Wist.Crypto.Properties;

namespace Wist.Crypto
{
    [RegisterExtension(typeof(ISigningService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class AccountSigningService : ISigningService
    {
        protected byte[] _secretKey;
        protected byte[] _expandedPrivateKey;
        private readonly IHashCalculation _defaultHash;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public AccountSigningService(IHashCalculationsRepository hashCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _defaultHash = hashCalculationRepository.Create(Globals.DEFAULT_HASH);
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            PublicKeys = new IKey[1];
        }

        public IKey[] PublicKeys { get; private set; }

        public string Name => GetType().Name;

        public virtual void Initialize(params byte[][] secretKeys)
        {
            if (secretKeys == null)
            {
                throw new ArgumentNullException(nameof(secretKeys));
            }

            if(secretKeys.Length != 1)
            {
                throw new WrongSecretKeysNumberException(nameof(AccountSigningService), 1);
            }

			_secretKey = Ed25519.SecretKeyFromSeed(secretKeys[0]);
			Ed25519.KeyPairFromSeed(out byte[] publicKey, out _expandedPrivateKey, secretKeys[0]);

            PublicKeys[0] = _identityKeyProvider.GetKey(publicKey);
        }

        public void Sign(IPacket packet, object args = null)
        {
            if (!(packet is SignedPacketBase packetBase))
            {
                throw new ArgumentOutOfRangeException(nameof(packet), string.Format(Resources.ERR_WRONG_PACKET_BASE_TYPE, nameof(AccountSigningService), typeof(SignedPacketBase).FullName));
            }

            byte[] message = packet.BodyBytes.ToArray();
            byte[] signature = Ed25519.Sign(message, _expandedPrivateKey);

            packetBase.Signer = PublicKeys[0];
            packetBase.Signature = signature;
        }

        public bool Verify(IPacket packet)
        {
            if (!(packet is SignedPacketBase packetBase))
            {
                throw new ArgumentOutOfRangeException(nameof(packet), string.Format(Resources.ERR_WRONG_PACKET_BASE_TYPE, nameof(AccountSigningService), typeof(SignedPacketBase).FullName));
            }

            byte[] signature = packetBase.Signature.ToArray();
            byte[] message = packetBase.BodyBytes.ToArray();
            byte[] publickKey = packetBase.Signer.Value.ToArray();

            return Ed25519.Verify(signature, message, publickKey);
        }

        public bool CheckTarget(params byte[][] targetValues)
        {
            if (targetValues == null)
            {
                throw new ArgumentNullException(nameof(targetValues));
            }

            if(targetValues.Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(targetValues));
            }

            return targetValues[0].Equals32(PublicKeys[0].Value.ToArray());
        }
    }

}
