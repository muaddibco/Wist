using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public abstract class TransactionalBlockBase : BlockSyncedBase
    {
        public override PacketType PacketType => PacketType.TransactionalChain;
    }
}
