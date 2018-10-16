using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Registry
{
    public class RegistryFullBlock : RegistryBlockBase
    {
        public override ushort BlockType => BlockTypes.Registry_FullBlock;

        public override ushort Version => 1;

        public SortedList<ushort, ITransactionRegistryBlock> TransactionHeaders { get; set; }

        public byte[] ShortBlockHash { get; set; }
    }
}
