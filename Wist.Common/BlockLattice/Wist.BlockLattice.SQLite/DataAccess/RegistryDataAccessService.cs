using Wist.BlockLattice.DataModel;

namespace Wist.BlockLattice.SQLite.DataAccess
{
    public partial class DataAccessService
    {
        public void AddRegistryFullBlock(ulong blockHeight, ushort shardId, byte[] content)
        {
            lock(_sync)
            {
                _dataContext.TransactionsRegistryBlocks.Add(new TransactionsRegistryBlock
                {
                    TransactionsRegistryBlockId = blockHeight,
                    ShardId = shardId,
                    Content = content
                });
            }
        }
    }
}
