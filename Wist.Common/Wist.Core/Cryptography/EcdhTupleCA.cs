namespace Wist.Core.Cryptography
{
    /// <summary>
    /// data for passing the asset to the receiver secretly
    /// If the pedersen commitment to an asset is C = aG + I,
    /// "Mask" contains a 32 byte key 'a'; Asset can be calculated from Commitment
    /// </summary>
    public class EcdhTupleCA
    {
        public byte[] Mask { get; set; }
        public byte[] AssetId { get; set; }
    }
}
