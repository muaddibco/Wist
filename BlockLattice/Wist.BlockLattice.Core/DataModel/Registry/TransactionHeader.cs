using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class TransactionHeader : IEqualityComparer<TransactionHeader>
    {
        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public ulong ReferencedHeight { get; set; }

        public byte[] ReferencedBodyHash { get; set; }

        public byte[] ReferencedTargetHash { get; set; }

        public bool Equals(TransactionHeader x, TransactionHeader y)
        {
            if (x != null && y != null)
            {
                return x.ReferencedPacketType == y.ReferencedPacketType && x.ReferencedBlockType == y.ReferencedBlockType && x.ReferencedHeight == y.ReferencedHeight;
            }

            return x == null && y == null;
        }

        public int GetHashCode(TransactionHeader obj)
        {
            return obj.ReferencedPacketType.GetHashCode() ^ obj.ReferencedBlockType.GetHashCode() ^ obj.ReferencedHeight.GetHashCode();
        }
    }
}
