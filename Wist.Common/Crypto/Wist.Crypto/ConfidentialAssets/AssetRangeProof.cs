namespace Wist.Crypto.ConfidentialAssets
{
    public class AssetRangeProof
    {
        public AssetRangeProof()
        {
            AssetCommitments = new byte[0][];
            Rs = new RingSignature();
        }

        internal byte[][] AssetCommitments { get; set; }
        public RingSignature Rs { get; set; }
    }
}
