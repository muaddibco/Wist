using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public class SynchronizationMedianApprovalBlock : SynchronizationBlockBase
    {
        public override ushort BlockType => BlockTypes.Synchronization_MedianApproval;

        public override ushort Version => 1;
    }
}
