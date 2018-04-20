using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel
{
    public class TransferFundsBlock : TransactionalBlockBase
    {
        public override TransactionalBlockType BlockType => TransactionalBlockType.TransferFunds;

        public override ushort Version => 1;

        /// <summary>
        /// 32 byte of Original Hash value of Transactional Account that is target of transaction
        /// </summary>
        public byte[] TargetOriginalHash { get; set; }
    }
}
