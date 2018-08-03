using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class SyncedBlockBase : SignedBlockBase
    {
        public ulong BlockHeight { get; set; }

        public ulong SyncBlockOrder { get; set; }

        public POWType POWType { get; set; }

        public ulong Nonce { get; set; }

        /// <summary>
        /// 64 byte value of hash of sum of Hash of referenced Sync Block Content and Nonce
        /// </summary>
        public byte[] HashNonce { get; set; }
    }
}
