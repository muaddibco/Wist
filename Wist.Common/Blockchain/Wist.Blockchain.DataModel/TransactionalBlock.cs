using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.Blockchain.DataModel
{
    [Table("transactional_chain_blocks")]
    public class TransactionalBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong TransactionalBlockId { get; set; }

        public TransactionalIdentity Identity { get; set; }

        public BlockHashKey HashKey { get; set; }

        public ulong SyncBlockHeight { get; set; }

        public ulong BlockHeight { get; set; }

        public ushort BlockType { get; set; }

        public byte[] BlockContent { get; set; }
    }
}
