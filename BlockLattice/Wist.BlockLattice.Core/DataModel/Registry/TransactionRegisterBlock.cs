using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class TransactionRegisterBlock : SyncedBlockBase
    {
        public override PacketType PacketType => PacketType.Registry;

        public override ushort BlockType => BlockTypes.Registry_TransactionRegister;

        public override ushort Version => 1;

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public ulong ReferencedHeight { get; set; }

        public byte[] ReferencedBodyHash { get; set; }
    }
}
