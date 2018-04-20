using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel
{
    public class GenesisBlock : TransactionalBlockBase
    {
        public override TransactionalBlockType BlockType => TransactionalBlockType.AccountBlock;

        public override ushort Version => 1;
    }
}
