using System.Collections.Generic;
using System.Linq;
using Wist.Blockchain.DataModel;

namespace Wist.Blockchain.SQLite.DataAccess
{
    public partial class DataAccessService
    {
        public void AddRegistryFullBlock(ulong syncBlockHeight, ulong round, int transactionsCount, byte[] content, byte[] hash)
        {
            lock(_sync)
            {
                _dataContext.TransactionsRegistryBlocks.Add(new TransactionsRegistryBlock
                {
                    SyncBlockHeight = syncBlockHeight,
                    Round = round,
                    TransactionsCount = transactionsCount,
                    Content = content,
                    Hash = hash
                });
            }
        }

        public List<TransactionsRegistryBlock> GetAllTransactionsRegistryBlocks()
        {
            lock (_sync)
            {
                return _dataContext.TransactionsRegistryBlocks.ToList();
            }
        }

        public TransactionsRegistryBlock GetTransactionsRegistryBlock(ulong syncBlockHeight, ulong round)
        {
            lock(_sync)
            {
                return _dataContext.TransactionsRegistryBlocks.FirstOrDefault(r => r.SyncBlockHeight == syncBlockHeight && r.Round == round);
            }
        }

        public List<TransactionsRegistryBlock> GetTransactionsRegistryBlocks(ulong syncBlockHeight)
        {
            lock (_sync)
            {
                return _dataContext.TransactionsRegistryBlocks.Where(r => r.SyncBlockHeight == syncBlockHeight).ToList();
            }
        }
    }
}
