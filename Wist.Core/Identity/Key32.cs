using Wist.Core.ExtensionMethods;

namespace Wist.Core.Identity
{
    /// <summary>
    /// Class represents Key with length of 32 bytes
    /// </summary>
    public class Key32 : IKey
    {
        public Key32()
        {

        }

        public Key32(byte[] value)
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

            Key32 pk1 = x as Key32;
            Key32 pk2 = y as Key32;

            if(pk1 == null || pk2 == null)
            {
                return false;
            }

            return pk1.Value.Equals32(pk2.Value);
        }

        public int GetHashCode(IKey obj)
        {
            return ((Key32)obj).Value.GetHashCode32();
        }

        public override int GetHashCode() => Value.GetHashCode32();

        public override string ToString() => Value.ToHexString();

        public override bool Equals(object obj)
        {
            Key32 pk = obj as Key32;

            if(pk == null)
            {
                return false;
            }

            return Value.Equals32(pk.Value);
        }
    }
}
