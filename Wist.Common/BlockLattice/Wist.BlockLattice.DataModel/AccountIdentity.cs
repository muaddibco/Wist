using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("account_identity")]
    public class AccountIdentity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountIdentityId { get; set; }

        public byte[] PublicKey { get; set; }
    }
}
