using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.MySql.DataModel
{
    [Table("transactional_chain_blocks")]
    public class TransactionalBlock
    {
        public long TransactionalBlockId { get; set; }

        public TransactionalGenesis TransactionalGenesis { get; set; }

        public uint BlockOrder { get; set; }
    }
}
