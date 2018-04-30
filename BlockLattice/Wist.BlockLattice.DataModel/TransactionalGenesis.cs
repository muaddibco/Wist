using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("transactional_chain_genesis")]
    public class TransactionalGenesis
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionalGenesisId { get; set; }

        public ushort Version { get; set; }

        [StringLength(128)]
        /// <summary>
        /// HEX string representation of 64-byte original HASH value associated with current Transactional Chain
        /// </summary>
        public string OriginalHash { get; set; }

        public byte[] BlockContent { get; set; }

        public virtual ICollection<TransactionalBlock> TransactionBlocks { get; set; }
    }
}
