using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class SyncedBlockBase : SignedBlockBase
    {
        public ulong BlockHeight { get; set; }

        public ulong SyncBlockHeight { get; set; }

        public uint Nonce { get; set; }

        /// <summary>
        /// 24 byte value of hash of sum of Hash of referenced Sync Block Content and Nonce
        /// </summary>
        public byte[] PowHash { get; set; }
    }
}
