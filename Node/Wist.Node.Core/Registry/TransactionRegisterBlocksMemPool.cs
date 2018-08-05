using System.Collections.Concurrent;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.MemPools;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegisterBlocksMemPool : MemPoolBase<TransactionRegisterBlock>
    {
        private readonly ConcurrentDictionary<int, TransactionRegisterBlock> _transactionRegisterBlocks;

        public override bool AddIfNotExist(TransactionRegisterBlock item)
        {
            TransactionRegisterBlock transactionRegisterBlock = _transactionRegisterBlocks.AddOrUpdate(item.GetHashCode(), item, (k, v) => v);

            return transactionRegisterBlock == item;
        }
    }
}
