using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.Blockchain.DataModel
{
    [Table("utxo_confidential_blocks")]
    public class UtxoConfidentialBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong UtxoConfidentialBlockId { get; set; }

        public BlockHashKey HashKey { get; set; }

        public ulong SyncBlockHeight { get; set; }

        public UtxoConfidentialKeyImage KeyImage { get; set; }

        public ushort BlockType { get; set; }

        public byte[] DestinationKey { get; set; }

        public byte[] BlockContent { get; set; }
    }
}
