using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("synchronization_blocks")]
    public class SynchronizationBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong SynchronizationBlockId { get; set; }

        public DateTime ReceiveTime { get; set; }

        public DateTime MedianTime { get; set; }

        public byte[] BlockContent { get; set; }
    }
}
