using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.MySql.DataModel
{
    [Table("transactional_chain_genesis")]
    public class TransactionalGenesis
    {
        public long TransactionalGenesisId { get; set; }

        public byte[] OriginalHash { get; set; }

        public virtual ICollection<TransactionalBlock> TransactionBlocks { get; set; }
    }
}
