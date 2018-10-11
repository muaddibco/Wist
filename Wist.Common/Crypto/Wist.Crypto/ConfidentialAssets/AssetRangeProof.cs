using Chaos.NaCl.Internal.Ed25519Ref10;

namespace Wist.Crypto.ConfidentialAssets
{
    public class AssetRangeProof
    {
        private byte[][] _h;
        private RingSignature _rs;

        public AssetRangeProof()
        {
            _h = new byte[0][];
            _rs = new RingSignature();
        }

        internal byte[][] H { get => _h; set => _h = value; }
        public RingSignature Rs { get => _rs; set => _rs = value; }
    }
}
