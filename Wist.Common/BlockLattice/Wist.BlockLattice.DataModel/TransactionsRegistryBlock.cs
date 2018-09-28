using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.DataModel
{
    [Table("transactions_registry_block")]
    public class TransactionsRegistryBlock
    {
        [Key, Column(Order = 0)]
        public ulong TransactionsRegistryBlockId { get; set; }

        [Key, Column(Order = 1)]
        public ushort ShardId { get; set; }

        public byte[] Content { get; set; }
    }
}
