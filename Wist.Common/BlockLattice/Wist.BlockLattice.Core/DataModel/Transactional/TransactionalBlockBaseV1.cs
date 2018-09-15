using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel.Transactional
{
    public abstract class TransactionalBlockBaseV1 : TransactionalBlockBase
    {
        public override ushort Version => 1;

        /// <summary>
        /// Up to date funds at last transactional block
        /// </summary>
        public ulong UptodateFunds { get; set; }
    }
}
