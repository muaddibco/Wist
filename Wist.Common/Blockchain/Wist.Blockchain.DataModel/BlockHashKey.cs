using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.Blockchain.DataModel
{
    [Table("block_hash_keys")]
    public class BlockHashKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong BlockHashKeyId { get; set; }

        public ulong SyncBlockHeight { get; set; }

        public byte[] Hash { get; set; }
    }
}
