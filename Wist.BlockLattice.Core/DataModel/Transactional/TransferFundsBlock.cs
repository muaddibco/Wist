using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public class TransferFundsBlock : TransactionalBlockBase
    {
        public override BlockType BlockType => BlockType.Transaction_TransferFunds;

        public override ushort Version => 1;

        /// <summary>
        /// 32 byte of Original Hash value of Transactional Account that is target of transaction
        /// </summary>
        public byte[] TargetOriginalHash { get; set; }
    }
}
