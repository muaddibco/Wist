using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wist.BlockLattice.DataModel
{
    [Table("transactions_registry_block")]
    public class TransactionsRegistryBlock
    {
        [Key]
        public ulong TransactionsRegistryBlockId { get; set; }

        public byte[] Content { get; set; }
    }
}
