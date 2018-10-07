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

        public TransactionalIdentity Identity { get; set; }

        public ulong BlockHeight { get; set; }

        public ushort BlockType { get; set; }

        public byte[] BlockContent { get; set; }
    }
}
