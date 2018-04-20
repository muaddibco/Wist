using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel
{
    public class AcceptFundsBlock : TransactionalBlockBase
    {
        public override TransactionalBlockType BlockType => TransactionalBlockType.AcceptFunds;

        public override ushort Version => 1;

        /// <summary>
        /// 32 byte of Original Hash value of Transactional Account that is source of transaction that Income Transaction relates to
        /// </summary>
        public byte[] SourceOriginalHash { get; set; }
    }
}
