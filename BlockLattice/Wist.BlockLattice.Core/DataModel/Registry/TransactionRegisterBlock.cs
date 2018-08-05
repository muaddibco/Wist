using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class TransactionRegisterBlock : SyncedBlockBase, IEqualityComparer<TransactionRegisterBlock>
    {
        public override PacketType PacketType => PacketType.Registry;

        public override ushort BlockType => BlockTypes.Registry_TransactionRegister;

        public override ushort Version => 1;

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public ulong ReferencedHeight { get; set; }

        public byte[] ReferencedBodyHash { get; set; }

        public bool Equals(TransactionRegisterBlock x, TransactionRegisterBlock y)
        {
            if(x != null && y != null)
            {
                return x.PacketType == y.PacketType && x.BlockType == y.BlockType && x.ReferencedPacketType == y.ReferencedPacketType && x.ReferencedBlockType == y.ReferencedBlockType && x.ReferencedHeight == y.ReferencedHeight;
            }

            return x == null && y == null;
        }

        public int GetHashCode(TransactionRegisterBlock obj)
        {
            return obj.PacketType.GetHashCode() ^ obj.BlockType.GetHashCode() ^ obj.ReferencedPacketType.GetHashCode() ^ obj.ReferencedBlockType.GetHashCode() ^ obj.ReferencedHeight.GetHashCode(); 
        }
    }
}
