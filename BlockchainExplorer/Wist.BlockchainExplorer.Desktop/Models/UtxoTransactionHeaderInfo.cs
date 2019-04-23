using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.BlockchainExplorer.Desktop.Models
{
    public class UtxoTransactionHeaderInfo : TransactionHeaderBase
    {
        public string TransactionKey { get; set; }

        public string KeyImage { get; set; }

        public string Target { get; set; }
    }
}
