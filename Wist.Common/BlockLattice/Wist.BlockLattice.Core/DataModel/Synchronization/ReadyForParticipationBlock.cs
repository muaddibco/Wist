using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    /// <summary>
    /// <see cref="ReadyForParticipationBlock"/> class required for letting know existing participants of Synchronization Group that new member is ready to participate in achieving consensus on Synchronization Block approval
    /// </summary>
    public class ReadyForParticipationBlock : SynchronizationBlockBase
    {
        public override PacketType PacketType => PacketType.Synchronization;

        public override ushort BlockType => BlockTypes.Synchronization_ReadyToParticipateBlock;

        public override ushort Version => 1;
    }
}
