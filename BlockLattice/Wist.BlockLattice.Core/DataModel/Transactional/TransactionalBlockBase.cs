using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public abstract class TransactionalBlockBase : BlockBase
    {
        public override ChainType ChainType => ChainType.TransactionalChain;
        /// <summary>
        /// 64 byte of Hash value 
        /// </summary>
        public byte[] NBackHash { get; set; }

        /// <summary>
        /// 64 byte of Hash value level N of account name encoded with Parent Account's Public Key
        /// </summary>
        public byte[] OriginalHash { get; set; }
    }
}
