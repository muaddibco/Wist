using Wist.BlockLattice.DataModel;

namespace Wist.BlockLattice.SQLite.DataAccess
{
    public partial class DataAccessService
    {
        public void AddRegistryFullBlock(ulong syncBlockHeight, ulong round, int transactionsCount, byte[] content)
        {
            lock(_sync)
            {
                _dataContext.TransactionsRegistryBlocks.Add(new TransactionsRegistryBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    Round = round,
                    TransactionsCount = transactionsCount,
                    Content = content
                });
            }
        }
    }
}
