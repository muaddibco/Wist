using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.Blockchain.DataModel
{
    [Table("account_identity")]
    public class AccountIdentity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong AccountIdentityId { get; set; }

        public ulong KeyHash { get; set; }

        public byte[] PublicKey { get; set; }
    }
}
