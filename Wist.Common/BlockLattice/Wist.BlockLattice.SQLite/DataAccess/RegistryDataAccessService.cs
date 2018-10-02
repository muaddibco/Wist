using Wist.BlockLattice.DataModel;

namespace Wist.BlockLattice.SQLite.DataAccess
{
    public partial class DataAccessService
    {
        public void AddRegistryFullBlock(ulong blockHeight, byte[] content)
        {
            lock(_sync)
            {
                _dataContext.TransactionsRegistryBlocks.Add(new TransactionsRegistryBlock
                {
                    TransactionsRegistryBlockId = blockHeight,
                    Content = content
                });
            }
        }
    }
}
