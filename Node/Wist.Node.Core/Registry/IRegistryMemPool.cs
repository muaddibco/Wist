using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Registry
{
    [ServiceContract]
    public interface IRegistryMemPool
    {
        bool EnqueueTransactionRegisterBlock(TransactionRegisterBlock transactionRegisterBlock);
        bool EnqueueTransactionsShortBlock(TransactionsShortBlock transactionsShortBlock);
        IEnumerable<TransactionRegisterBlock> DequeueBulk(int maxCount);
        int GetConfidenceRate(TransactionsShortBlock transactionsShortBlock);
        void ClearByConfirmed(TransactionsShortBlock transactionsShortBlock);
    }
}
