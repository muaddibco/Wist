using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.MySql.DataModel
{
    [Table("transactional_blocks")]
    public class TransactionalBlock
    {
        public byte[] OriginalHash { get; set; }

        public uint BlockOrder { get; set; }
    }
}
