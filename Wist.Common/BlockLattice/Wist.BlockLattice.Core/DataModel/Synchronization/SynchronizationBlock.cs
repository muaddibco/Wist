using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public class SynchronizationProducingBlock : SynchronizationBlockBase
    {
        public override ushort BlockType => BlockTypes.Synchronization_TimeSyncProducingBlock;

        public override ushort Version => 1;

        public ushort Round { get; set; }
    }
}
