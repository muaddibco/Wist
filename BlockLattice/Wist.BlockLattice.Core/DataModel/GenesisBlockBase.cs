using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class GenesisBlockBase : BlockBase
    {
        public GenesisBlockBase()
        {
            BlockOrder = 0;
        }

        public override ushort BlockType => BlockTypes.Genesis;
    }
}
