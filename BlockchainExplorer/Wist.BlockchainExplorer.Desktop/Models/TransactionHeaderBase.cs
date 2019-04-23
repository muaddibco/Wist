using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wist.Blockchain.Core.Enums;

namespace Wist.BlockchainExplorer.Desktop.Models
{
    public abstract class TransactionHeaderBase
    {
        public int OrderInBlock { get; set; }

        public ulong SyncBlockHeight { get; set; }

        public PacketType PacketType { get; set; }

        public ushort BlockType { get; set; }
    }
}
