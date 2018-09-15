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

        public AccountIdentity Identity { get; set; }

        public uint BlockOrder { get; set; }

        public ushort Version { get; set; }

        public byte[] BlockContent { get; set; }

        public TransactionalGenesis TransactionalGenesis { get; set; }

        public virtual ICollection<TransactionalBlock> TransactionBlocks { get; set; }
    }
}
