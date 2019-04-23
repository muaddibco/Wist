namespace Wist.Core.Cryptography
{ 
    /// <summary>
    /// data for passing the amount and asset to the receiver secretly
    /// If the pedersen commitment to an amount of asset is C = aG + bH,
    /// "Mask" contains a 32 byte key 'a';
    /// "Amount" contains a hex representation (in 32 bytes) of a 64 bit number 'b'
    /// "Asset" contains  a hex representation (in 32 bytes) of an asset id
    /// </summary>
    public class EcdhTupleCTA
    {
        public byte[] Mask { get; set; }
        public byte[] Amount { get; set; }
        public byte[] AssetId { get; set; }
    }
}
