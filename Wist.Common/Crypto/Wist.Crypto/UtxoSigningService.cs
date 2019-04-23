using System;
using System.Linq;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.Models;
using Wist.Core.ExtensionMethods;
using Wist.Crypto.ConfidentialAssets;
using Wist.Crypto.Exceptions;
using Wist.Crypto.Properties;

namespace Wist.Crypto
{
    [RegisterExtension(typeof(ISigningService), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class UtxoSigningService : ISigningService
    {
        protected byte[] _secretSpendKey;
		protected byte[] _secretViewKey;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public UtxoSigningService(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            PublicKeys = new IKey[2];
        }

        public IKey[] PublicKeys { get; private set; }

        public string Name => GetType().Name;

        public virtual void Initialize(params byte[][] secretKeys)
        {
            if (secretKeys == null)
            {
                throw new ArgumentNullException(nameof(secretKeys));
            }

            if (secretKeys.Length != 2)
            {
                throw new WrongSecretKeysNumberException(nameof(AccountSigningService), 2);
            }

            _secretSpendKey = secretKeys[0];
            _secretViewKey = secretKeys[1];
            PublicKeys[0] = _identityKeyProvider.GetKey(ConfidentialAssetsHelper.GetPublicKey(_secretSpendKey));
            PublicKeys[1] = _identityKeyProvider.GetKey(ConfidentialAssetsHelper.GetPublicKey(_secretViewKey));
        }

        public void Sign(IPacket packet, object args = null)
        {
            if (!(packet is UtxoSignedPacketBase packetBase))
            {
                throw new ArgumentOutOfRangeException(nameof(packet), string.Format(Resources.ERR_WRONG_PACKET_BASE_TYPE, nameof(UtxoSigningService), typeof(UtxoSignedPacketBase).FullName));
            }

            UtxoSignatureInput signatureInput = args as UtxoSignatureInput;

            byte[][] publicKeys = signatureInput.PublicKeys;
            int index = signatureInput.KeyPosition;
            byte[] otsk = ConfidentialAssetsHelper.GetOTSK(signatureInput.SourceTransactionKey, _secretViewKey, _secretSpendKey);

            byte[] keyImage = ConfidentialAssetsHelper.GenerateKeyImage(otsk);
            packetBase.KeyImage = _identityKeyProvider.GetKey(keyImage);

            byte[] msg = new byte[packet.BodyBytes.Length + keyImage.Length];

            Array.Copy(packet.BodyBytes.ToArray(), 0, msg, 0, packet.BodyBytes.Length);
            Array.Copy(keyImage, 0, msg, packet.BodyBytes.Length, keyImage.Length);

            RingSignature[] ringSignatures = ConfidentialAssetsHelper.GenerateRingSignature(msg, keyImage, publicKeys, otsk, index);

            packetBase.PublicKeys = signatureInput.PublicKeys.Select(p => _identityKeyProvider.GetKey(p)).ToArray();
            packetBase.Signatures = ringSignatures;
        }

        public bool Verify(IPacket packet)
        {
            if (!(packet is UtxoSignedPacketBase packetBase))
            {
                throw new ArgumentOutOfRangeException(nameof(packet), string.Format(Resources.ERR_WRONG_PACKET_BASE_TYPE, nameof(UtxoSigningService), typeof(UtxoSignedPacketBase).FullName));
            }

            byte[] msg = packetBase.BodyBytes.ToArray();
            byte[] keyImage = packetBase.KeyImage.Value.ToArray();
            IKey[] publicKeys = packetBase.PublicKeys;
            RingSignature[] signatures = packetBase.Signatures;

            return ConfidentialAssetsHelper.VerifyRingSignature(msg, keyImage, publicKeys.Select(p => p.Value.ToArray()).ToArray(), signatures);
        }

        public bool CheckTarget(params byte[][] targetValues)
        {
            if (targetValues == null)
            {
                throw new ArgumentNullException(nameof(targetValues));
            }

            if (targetValues.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(targetValues));
            }

            return ConfidentialAssetsHelper.IsDestinationKeyMine(targetValues[0], targetValues[1], _secretViewKey, PublicKeys[0].Value.ToArray());
        }
    }
}

