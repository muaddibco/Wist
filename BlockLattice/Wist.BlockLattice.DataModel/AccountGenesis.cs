using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.MySql.DataModel
{
    [Table("account_chain_genesis")]
    public class AccountGenesis
    {
        public long AccountGenesisId { get; set; }

        public virtual ICollection<AccountBlock> AccountBlocks { get; set; }
    }
}
