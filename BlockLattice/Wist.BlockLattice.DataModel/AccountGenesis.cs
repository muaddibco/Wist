using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("account_chain_genesis")]
    public class AccountGenesis
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountGenesisId { get; set; }

        public virtual ICollection<AccountBlock> AccountBlocks { get; set; }
    }
}
