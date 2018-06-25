using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wist.BlockLattice.DataModel
{
    //TODO: make this table encrypted
    [Table("account_seeds")]
    public class AccountSeed
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountSeedId { get; set; }

        public byte[] Seed { get; set; }

        public AccountIdentity Identity { get; set; }
    }
}
