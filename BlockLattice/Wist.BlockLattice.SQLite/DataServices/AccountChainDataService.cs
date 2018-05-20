using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Account;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class AccountChainDataService : IChainDataService
    {
        public ChainType ChainType => ChainType.AccountChain;

        public void AddBlock(BlockBase block)
        {
            throw new NotImplementedException();
        }

        public void CreateGenesisBlock(GenesisBlockBase genesisBlock)
        {
            throw new NotImplementedException();
        }

        public bool DoesChainExist(byte[] key)
        {
            throw new NotImplementedException();
        }

        public BlockBase[] GetAllBlocks(byte[] key)
        {
            throw new NotImplementedException();
        }

        public List<BlockBase> GetAllLastBlocksByType(ushort blockType)
        {
            throw new NotImplementedException();
        }

        public BlockBase GetBlockByOrder(byte[] key, uint order)
        {
            throw new NotImplementedException();
        }

        public GenesisBlockBase GetGenesisBlock(byte[] key)
        {
            throw new NotImplementedException();
        }

        public BlockBase GetLastBlock(byte[] key)
        {
            throw new NotImplementedException();
        }
    }
}
