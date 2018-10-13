namespace Wist.Core.Cryptography
{
    public class BorromeanRingSignature
    {
        public BorromeanRingSignature()
        {
            E = new byte[32];
            S = new byte[0][];
        }

        public BorromeanRingSignature(int length)
        {
            E = new byte[32];
            S = new byte[length][];

            for (int i = 0; i < length; i++)
            {
                S[i] = new byte[32];
            }
        }

        public byte[] E { get; set; }
        public byte[][] S { get ; set; }
    }
}
