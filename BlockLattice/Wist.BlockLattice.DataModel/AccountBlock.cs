using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.MySql.DataModel
{
    [Table("account_chain_blocks")]
    public class AccountBlock
    {
        public long AccountBlockId { get; set; }

        public AccountGenesis AccountGenesis { get; set; }
    }
}
