using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public class SynchronizationConfirmedBlock : BlockBase
    {
        public override PacketType PacketType => PacketType.Synchronization;

        public override ushort BlockType => BlockTypes.Synchronization_ConfirmedBlock;

        public override ushort Version => 1;

        public DateTime[] ReportedTimes { get; set; }

        public byte[][] Signatures { get; set; }

        public byte[][] PublicKeys { get; set; }
    }
}
