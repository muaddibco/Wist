using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wist.BlockLattice.DataModel
{
    //TODO: make this table encrypted
    /// <summary>
    /// This table contains seeds (Private Keys) for accounts used in testing and simulations
    /// </summary>
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
