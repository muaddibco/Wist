using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wist.BlockLattice.DataModel
{
    [Table("transactional_identity")]
    public class TransactionalIdentity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong TransactionalPublicKeyId { get; set; }

        public AccountIdentity Identity { get; set; }

        public virtual ICollection<TransactionalBlock> Blocks { get; set; }
    }
}
