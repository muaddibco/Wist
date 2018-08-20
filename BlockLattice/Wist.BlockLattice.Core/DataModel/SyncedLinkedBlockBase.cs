using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class SyncedLinkedBlockBase : SyncedBlockBase
    {
        /// <summary>
        /// 64 byte value of hash of previous block content
        /// </summary>
        public byte[] HashPrev { get; set; }
    }
}
