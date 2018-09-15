using Wist.BlockLattice.Core.Enums;

namespace Wist.BlockLattice.Core.DataModel.Storage
{
    public class StorageTransactionFullBlock : StorageBlockBase
    {
        public override ushort BlockType => BlockTypes.Storage_TransactionFull;

        public override ushort Version => 1;

        public BlockBase Transaction { get; set; }
    }
}
