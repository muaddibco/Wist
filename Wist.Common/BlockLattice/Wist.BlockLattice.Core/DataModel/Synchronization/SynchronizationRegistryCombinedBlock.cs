using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Synchronization
{
    public class SynchronizationRegistryCombinedBlock : SynchronizationBlockBase
    {
        public override ushort BlockType => BlockTypes.Synchronization_RegistryCombinationBlock;

        public override ushort Version => 1;

        public byte[][] BlockHashes { get; set; }
    }
}
