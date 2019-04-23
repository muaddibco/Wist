namespace Wist.Core.Cryptography
{
    /// <summary>
    /// data for passing the asset to the receiver secretly
    /// If the pedersen commitment to an asset is C = aG + I,
    /// "Mask" contains a 32 byte key 'a'; Asset can be calculated from Commitment
    /// </summary>
    public class EcdhTupleIP
    {
        public byte[] Issuer { get; set; }
        public byte[] Payload { get; set; }
    }
}
