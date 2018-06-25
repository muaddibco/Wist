using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    /// <summary>
    /// TransactionalGenesis table intended for storing lastly modified edition of Transactional Genesis Block
    /// </summary>
    [Table("transactional_chain_genesis")]
    public class TransactionalGenesis
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionalGenesisId { get; set; }

        public AccountIdentity Identity { get; set; }

        public ushort Version { get; set; }

        public byte[] BlockContent { get; set; }

        public virtual ICollection<TransactionalGenesisModification> GenesisModifications { get; set; }
    }
}
