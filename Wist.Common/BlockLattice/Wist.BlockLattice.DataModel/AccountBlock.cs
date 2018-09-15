using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("account_chain_blocks")]
    public class AccountBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountBlockId { get; set; }

        public AccountGenesis AccountGenesis { get; set; }
    }
}
