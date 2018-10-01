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
        // Primary key defined using Fluent API
        public ulong TransactionsRegistryBlockId { get; set; }

        // Primary key defined using Fluent API
        public ushort ShardId { get; set; }

        public byte[] Content { get; set; }
    }
}
