using System;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public abstract class SynchronizationBlockBase : SyncedLinkedBlockBase
    {
        public override PacketType PacketType => PacketType.Synchronization;

        public DateTime ReportedTime { get; set; }
    }
}
