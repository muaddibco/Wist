using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Core.Cryptography
{
    [ServiceContract]
    public interface IUtxoConfidentialCryptoService
    {
        IKey PublicViewKey { get; }
        IKey PublicSpendKey { get; }

        RingSignature[] Sign(byte[] msg, byte[] keyImage, IKey[] publicKeys, byte[] secretKey, int index);

        void Initialize(byte[] privateViewKey, byte[] privateSpendKey);
        void GetRandomKeyPair(out byte[] secretKey, out byte[] publicKey);
    }
}
