using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.MemPools
{
    [RegisterExtension(typeof(IMemPool), Lifetime = LifetimeManagement.Singleton]
    public class TransactionRegisterBlocksMemPool : MemPoolBase<TransactionRegisterBlock>
    {
        private readonly ConcurrentDictionary<int, TransactionRegisterBlock> _transactionRegisterBlocks;

        public override void AddIfNotExist(TransactionRegisterBlock item)
        {
            if(!_transactionRegisterBlocks.ContainsKey(item.GetHashCode()))
            {
                _transactionRegisterBlocks.AddOrUpdate(item.GetHashCode(), item, (k, v) => v);
            }
        }
    }
}
