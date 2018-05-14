using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public class SynchronizationBlockV1 : BlockBase
    {
        public override ChainType ChainType => ChainType.Synchronization;

        public override ushort BlockType => BlockTypes.Synchronization_TimeSyncBlock;

        public override ushort Version => 1;

        public DateTime ReportedTime { get; set; }

        public byte[] Signature { get; set; }

        public byte[] PublicKey { get; set; }
    }
}
