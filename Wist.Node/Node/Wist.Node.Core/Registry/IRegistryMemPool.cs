using System.Collections.Generic;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Core.Architecture;

namespace Wist.Node.Core.Registry
{
    [ServiceContract]
    public interface IRegistryMemPool
    {
        bool EnqueueTransactionWitness(RegistryRegisterBlock transactionWitness);
        bool EnqueueTransactionWitness(RegistryRegisterUtxoConfidential transactionWitness);

        SortedList<ushort, RegistryRegisterBlock> DequeueStateWitnessBulk();
        SortedList<ushort, RegistryRegisterUtxoConfidential> DequeueUtxoWitnessBulk();

        void ClearWitnessed(RegistryShortBlock transactionsShortBlock);
    }
}
