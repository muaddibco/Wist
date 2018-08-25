using Wist.Core.ExtensionMethods;

namespace Wist.Core.Identity
{
    /// <summary>
    /// Class represents Public Key with length of 32 bytes
    /// </summary>
    public class Public32Key : IKey
    {
        public Public32Key()
        {

        }

        public Public32Key(byte[] value)
        {
            Value = value;
        }

        /// <summary>
        /// Byte array of length of 32 bytes
        /// </summary>
        public byte[] Value { get; set; } //TODO: need to add length check at setter

        public int Length => 32;

        public bool Equals(IKey x, IKey y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            Public32Key pk1 = x as Public32Key;
            Public32Key pk2 = y as Public32Key;

            if(pk1 == null || pk2 == null)
            {
                return false;
            }

            return pk1.Value.Equals32(pk2.Value);
        }

        public int GetHashCode(IKey obj)
        {
            return ((Public32Key)obj).Value.GetHashCode32();
        }

        public override int GetHashCode() => Value.GetHashCode32();

        public override string ToString() => Value.ToHexString();

        public override bool Equals(object obj)
        {
            Public32Key pk = obj as Public32Key;

            if(pk == null)
            {
                return false;
            }

            return Value.Equals32(pk.Value);
        }
    }
}
