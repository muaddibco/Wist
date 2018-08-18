using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class TransactionsFullBlock : RegistryBlockBase
    {
        public override ushort BlockType => BlockTypes.Registry_TransactionFullBlock;

        public override ushort Version => 1;

        public SortedList<int, TransactionHeader> TransactionHeaders { get; set; }
    }
}
