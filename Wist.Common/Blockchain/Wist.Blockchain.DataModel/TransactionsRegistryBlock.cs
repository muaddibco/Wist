using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wist.Blockchain.DataModel
{
    [Table("transactions_registry_block")]
    public class TransactionsRegistryBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong TransactionsRegistryBlockId { get; set; }

        public ulong SyncBlockHeight { get; set; }

        public ulong Round { get; set; }

        public int TransactionsCount { get; set; }

        public byte[] Content { get; set; }

        public byte[] Hash { get; set; }
    }
}
