using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public abstract class SynchronizationBlockBase : SyncedBlockBase
    {
        public SynchronizationBlockBase() => POWType = Wist.Core.ProofOfWork.POWType.None;

        public override PacketType PacketType => PacketType.Synchronization;

        public DateTime ReportedTime { get; set; }
    }
}
