using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel
{
    public abstract class TransactionalBlockBase
    {
        public abstract TransactionalBlockType BlockType { get; }

        public abstract ushort Version { get; }

        public uint BlockOrder { get; set; }

        /// <summary>
        /// 32 byte of Hash value level N of account name encoded with Parent Account's Public Key
        /// </summary>
        public byte[] OriginalHash { get; set; }

        /// <summary>
        /// Up to date funds at last transactional block
        /// </summary>
        public double UptodateFunds { get; set; }
    }
}
