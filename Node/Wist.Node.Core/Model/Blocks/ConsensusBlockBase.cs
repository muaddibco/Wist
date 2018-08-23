using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;

namespace Wist.Node.Core.Model.Blocks
{
    public abstract class ConsensusBlockBase : SignedBlockBase
    {
        public override PacketType PacketType => PacketType.Consensus;
    }
}
