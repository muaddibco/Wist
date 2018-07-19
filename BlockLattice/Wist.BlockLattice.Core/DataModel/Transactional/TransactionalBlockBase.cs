using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public abstract class TransactionalBlockBase : SyncedBlockBase
    {
        public override PacketType PacketType => PacketType.TransactionalChain;
    }
}
