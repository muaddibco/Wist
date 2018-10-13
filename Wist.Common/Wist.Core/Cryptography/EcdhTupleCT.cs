namespace Wist.Core.Cryptography
{
    /// <summary>
    /// data for passing the amount to the receiver secretly
    /// If the pedersen commitment to an amount is C = aG + bH,
    /// "Mask" contains a 32 byte key 'a';
    /// "Amount" contains a hex representation (in 32 bytes) of a 64 bit number 'b'
    /// </summary>
    public class EcdhTupleCT
    {
        public byte[] Mask { get; set; }
        public byte[] Amount { get; set; }
    }
}
