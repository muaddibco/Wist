using System.Collections.Generic;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Registry
{
    [ServiceContract]
    public interface IRegistryMemPool
    {
        bool EnqueueTransactionRegisterBlock(RegistryRegisterBlock transactionRegisterBlock);
        bool EnqueueTransactionsShortBlock(RegistryShortBlock transactionsShortBlock);
        SortedList<ushort, RegistryRegisterBlock> DequeueBulk(int maxCount);
        byte[] GetConfidenceMask(RegistryShortBlock transactionsShortBlock, out byte[] bitMask);
        void ClearByConfirmed(RegistryShortBlock transactionsShortBlock);
    }
}
