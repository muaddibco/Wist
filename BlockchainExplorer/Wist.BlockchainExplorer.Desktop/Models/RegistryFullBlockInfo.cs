using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.BlockchainExplorer.Desktop.Models
{
    public class RegistryFullBlockInfo
    {
        public ulong SyncBlockHeight { get; set; }
        public ulong Round { get; set; }
        public uint TransactionsCount { get; set; }

    }
}
