using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("transactional_chain_blocks")]
    public class TransactionalBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionalBlockId { get; set; }

        public TransactionalGenesis TransactionalGenesis { get; set; }

        public uint BlockOrder { get; set; }

        public uint ForkId { get; set; }

        public ushort BlockType { get; set; }

        public byte[] BlockContent { get; set; }
    }
}
