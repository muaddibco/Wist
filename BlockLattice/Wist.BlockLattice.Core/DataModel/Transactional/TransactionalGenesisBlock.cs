using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public class TransactionalGenesisBlock : GenesisBlockBase
    {
        public override ChainType ChainType => ChainType.TransactionalChain;

        public override ushort Version => 1;

        /// <summary>
        /// 64 byte of Hash value level N of account name encoded with Parent Account's Public Key
        /// </summary>
        public byte[] OriginalHash { get; set; }
    }
}
