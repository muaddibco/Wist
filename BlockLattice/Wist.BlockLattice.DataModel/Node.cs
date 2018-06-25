using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("nodes")]
    public class Node
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NodeId { get; set; }

        public AccountIdentity Identity { get; set; }

        public byte[] IPAddress { get; set; }
    }
}
