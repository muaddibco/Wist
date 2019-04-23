using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wist.BlockchainExplorer.Desktop.Models
{
    public class CombinedBlockInfo
    {
        public CombinedBlockInfo()
        {
            RegistryFullBlockInfos = new List<RegistryFullBlockInfo>();
        }

        public ulong SyncBlockHeight { get; set; }
        public ulong BlockHeight { get; set; }
        public uint CombinedRegistryBlocksCount { get; set; }

        public List<RegistryFullBlockInfo> RegistryFullBlockInfos { get; set; }
    }
}
