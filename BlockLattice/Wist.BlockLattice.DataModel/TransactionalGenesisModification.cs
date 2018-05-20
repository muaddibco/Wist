using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("transactional_chain_genesis_modifications")]
    public class TransactionalGenesisModification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionalGenesisModificationId { get; set; }

        public uint BlockOrder { get; set; }

        public ushort Version { get; set; }

        [StringLength(128)]
        
        /// <summary>
        /// HEX string representation of 64-byte original HASH value associated with current Transactional Chain
        /// </summary>
        public string OriginalHash { get; set; }

        public byte[] BlockContent { get; set; }

        public TransactionalGenesis TransactionalGenesis { get; set; }
    }
}
