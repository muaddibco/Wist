using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class TransactionsShortBlock : RegistryBlockBase
    {
        public override ushort BlockType => BlockTypes.Registry_TransactionShortBlock;

        public override ushort Version => 1;

        public SortedList<int, byte[]> TransactionHeaderHashes { get; set; }
    }
}
