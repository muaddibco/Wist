using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wist.Blockchain.DataModel
{
    [Table("nodes")]
    public class NodeRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong NodeId { get; set; }

        public AccountIdentity Identity { get; set; }

        public byte[] IPAddress { get; set; }

        public byte NodeRole { get; set; }
    }
}
