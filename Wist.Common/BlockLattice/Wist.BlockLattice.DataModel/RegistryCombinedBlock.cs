using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("registry_combined_blocks")]
    public class RegistryCombinedBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong RegistryCombinedBlockId { get; set; }


        public ulong SyncBlockHeight { get; set; }

        public ulong Round { get; set; }

        public byte[] Content { get; set; }
    }
}
