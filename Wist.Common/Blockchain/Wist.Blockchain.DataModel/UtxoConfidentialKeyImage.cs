using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.Blockchain.DataModel
{
    [Table("utxo_confidential_key_images")]
    public class UtxoConfidentialKeyImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong UtxoConfidentialKeyImageId { get; set; }

        public byte[] KeyImage { get; set; }
    }
}
