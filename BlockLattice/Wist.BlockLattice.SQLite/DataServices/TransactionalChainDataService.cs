using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Enums;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDataService : IChainDataService<TransactionalGenesisBlock, TransactionalBlockBase>
    {
        public ChainType ChainType => ChainType.TransactionalChain;

        public void AddBlock(TransactionalBlockBase block)
        {
            throw new NotImplementedException();
        }

        public void CreateGenesisBlock(TransactionalGenesisBlock genesisBlock)
        {
            throw new NotImplementedException();
        }

        public bool DoesChainExist(byte[] key)
        {
            throw new NotImplementedException();
        }

        public TransactionalBlockBase[] GetAllBlocks(byte[] key)
        {
            throw new NotImplementedException();
        }

        public TransactionalBlockBase GetBlockByOrder(byte[] key, uint order)
        {
            throw new NotImplementedException();
        }

        public TransactionalGenesisBlock GetGenesisBlock(byte[] key)
        {
            throw new NotImplementedException();
        }

        public TransactionalBlockBase GetLastBlock(byte[] key)
        {
            throw new NotImplementedException();
        }
    }
}
