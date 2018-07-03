using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class BlockSyncedBase : SignedBlockBase
    {
        public uint SyncBlockOrder { get; set; }

        /// <summary>
        /// 64? byte value
        /// </summary>
        public byte[] Nonce { get; set; }

        /// <summary>
        /// 64 byte value
        /// </summary>
        public byte[] HashNonce { get; set; }
    }
}
