using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public class SynchronizationBlock : SynchronizationBlockBase
    {
        public override ushort BlockType => BlockTypes.Synchronization_TimeSyncBlock;

        public override ushort Version => 1;
    }
}
