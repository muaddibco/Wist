using System.Collections.Generic;

namespace Wist.BlockchainExplorer.Desktop.Models
{
    public class BulkUpdate
    {
        public BulkUpdate()
        {
            SyncBlockInfos = new List<SyncBlockInfo>();
            CombinedBlockInfos = new List<CombinedBlockInfo>();
        }

        public List<SyncBlockInfo> SyncBlockInfos { get; set; }

        public List<CombinedBlockInfo> CombinedBlockInfos { get; set; }
    }
}
