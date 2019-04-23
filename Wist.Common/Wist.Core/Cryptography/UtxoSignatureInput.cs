using Wist.Core.Identity;

namespace Wist.Core.Cryptography
{
    public class UtxoSignatureInput
    {
        public UtxoSignatureInput(byte[] sourceTransactionKey, byte[][] publicKeys, int keyPosition)
        {
            SourceTransactionKey = sourceTransactionKey;
            PublicKeys = publicKeys;
            KeyPosition = keyPosition;
        }

        public byte[][] PublicKeys { get;  }
        public int KeyPosition { get; }
        public byte[] SourceTransactionKey { get; }
    }
}
