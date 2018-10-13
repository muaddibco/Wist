namespace Wist.Core.Cryptography
{
    public class SurjectionProof
    {
        public SurjectionProof()
        {
            AssetCommitments = new byte[0][];
            Rs = new BorromeanRingSignature();
        }

        public byte[][] AssetCommitments { get; set; }
        public BorromeanRingSignature Rs { get; set; }
    }
}
