using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Wist.BlockLattice.MySql.DataModel
{
    [Table("transactional_accounts")]
    public class TransactionalAccount
    {
        public byte[] OriginalHash { get; set; }
    }
}
