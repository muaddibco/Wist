using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Account;
using Wist.BlockLattice.Core.DataModel.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class AccountChainDataService : IChainDataService<AccountGenesisBlock, AccountBlockBase>
    {
        public ChainType ChainType => ChainType.AccountChain;

        public void AddBlock(AccountBlockBase block)
        {
            throw new NotImplementedException();
        }

        public void CreateGenesisBlock(AccountGenesisBlock genesisBlock)
        {
            throw new NotImplementedException();
        }

        public bool DoesChainExist(byte[] key)
        {
            throw new NotImplementedException();
        }

        public AccountBlockBase[] GetAllBlocks(byte[] key)
        {
            throw new NotImplementedException();
        }

        public AccountBlockBase GetBlockByOrder(byte[] key, uint order)
        {
            throw new NotImplementedException();
        }

        public AccountGenesisBlock GetGenesisBlock(byte[] key)
        {
            throw new NotImplementedException();
        }

        public AccountBlockBase GetLastBlock(byte[] key)
        {
            throw new NotImplementedException();
        }
    }
}
