using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class GenesisBlockBase : BlockSyncedBase
    {
        public GenesisBlockBase()
        {
            BlockHeight = 0;
        }

        public override ushort BlockType => BlockTypes.Genesis;

        public ulong ParkedFunds { get; set; }

        public IKey DelegatedAccount { get; set; }
    }
}
