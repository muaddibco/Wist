using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public class TransactionalDposVote : TransactionalBlockBase
    {
        public override ushort BlockType => BlockTypes.Transaction_Dpos;

        public override ushort Version => 1;

        /// <summary>
        /// 32 byte value of Public Key of Node that this chain votes for
        /// </summary>
        public byte[] NodePublickKey { get; set; }
    }
}
