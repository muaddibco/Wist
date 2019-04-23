using System;
using Wist.Core.ExtensionMethods;

namespace Wist.Core.Identity
{
    public class Key16 : IKey
    {
        private Memory<byte> _value;

        public Key16()
        {

        }

        public Key16(Memory<byte> value)
        {
            Value = value;
        }

        /// <summary>
        /// Byte array of length of 16 bytes
        /// </summary>
        public Memory<byte> Value
        {
            get => _value;
            set
            {
                _value = value;
                ArraySegment = _value.ToArraySegment();
            }
        } //TODO: need to add length check at setter

        public int Length => 16;

        public ArraySegment<byte> ArraySegment { get; private set; }

        public bool Equals(IKey x, IKey y)
        {
            if (x == null && y == null)
            {
                return true;
            }

            Key16 pk1 = x as Key16;
            Key16 pk2 = y as Key16;

            if (pk1 == null || pk2 == null)
            {
                return false;
            }

            return pk1.Value.EqualsX16(pk2.Value);
        }

        public int GetHashCode(IKey obj)
        {
            return ((Key16)obj).Value.GetHashCode16();
        }

        public override int GetHashCode() => Value.GetHashCode16();

        public override string ToString() => Value.ToHexString();

        public override bool Equals(object obj)
        {

            if (!(obj is Key16 pk))
            {
                return false;
            }

            return Value.EqualsX16(pk.Value);
        }

        public bool Equals(IKey other)
        {
            if (other == null)
            {
                return false;
            }

            return Value.EqualsX16(other.Value);
        }
    }
}
