using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.BlockchainExplorer.Desktop.Models
{
    public class AccountedTransactionHeaderInfo : TransactionHeaderBase
    {
        public ulong BlockHeight { get; set; }

        public string Target { get; set; }
    }
}
