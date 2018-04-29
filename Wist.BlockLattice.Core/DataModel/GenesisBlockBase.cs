using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Enums;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class GenesisBlockBase : BlockBase
    {
        public GenesisBlockBase()
        {
            BlockOrder = 0;
        }

        public override BlockType BlockType => BlockType.Genesis;

        public abstract ChainType ChainType { get; }
    }
}
