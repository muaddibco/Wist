﻿using Wist.Core.ExtensionMethods;

namespace Wist.Core.Identity
{
    public class Key16 : IKey
    {
        /// <summary>
        /// Byte array of length of 16 bytes
        /// </summary>
        public byte[] Value { get; set; } //TODO: need to add length check at setter

        public int Length => 16;

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
            Key16 pk = obj as Key16;

            if (pk == null)
            {
                return false;
            }

            return Value.EqualsX16(pk.Value);
        }
    }
}