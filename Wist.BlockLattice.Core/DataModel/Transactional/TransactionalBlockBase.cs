using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public abstract class TransactionalBlockBase : BlockBase
    {
        /// <summary>
        /// 64 byte of Hash value level N of account name encoded with Parent Account's Public Key
        /// </summary>
        public byte[] OriginalHash { get; set; }

        /// <summary>
        /// Up to date funds at last transactional block
        /// </summary>
        public double UptodateFunds { get; set; }
    }
}
