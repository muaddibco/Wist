using System;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.Enums;

namespace Wist.Blockchain.Core.DataModel.Synchronization
{
    public class SynchronizationProducingBlock : SynchronizationBlockBase
    {
        public override ushort BlockType => BlockTypes.Synchronization_TimeSyncProducingBlock;

        public override ushort Version => 1;

        public ushort Round { get; set; }
    }
}
