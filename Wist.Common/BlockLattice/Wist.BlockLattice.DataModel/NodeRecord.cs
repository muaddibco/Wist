using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wist.BlockLattice.DataModel
{
    [Table("nodes")]
    public class NodeRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NodeId { get; set; }

        public AccountIdentity Identity { get; set; }

        public byte[] IPAddress { get; set; }

        public byte NodeRole { get; set; }
    }
}
