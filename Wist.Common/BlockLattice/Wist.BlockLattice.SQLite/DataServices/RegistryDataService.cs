using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.SQLite.DataAccess;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryDataService : IChainDataService
    {
        private readonly ITranslatorsRepository _translatorsRepository;

        public RegistryDataService(ITranslatorsRepository translatorsRepository)
        {
            _translatorsRepository = translatorsRepository;
        }

        public PacketType ChainType => PacketType.Registry;

        public void Add(BlockBase item)
        {
            if(item is RegistryFullBlock block)
            {
                //TODO: shardId must be taken from somewhere
                DataAccessService.Instance.AddRegistryFullBlock(block.BlockHeight, 1, block.BodyBytes);
            }
        }

        public void CreateGenesisBlock(GenesisBlockBase genesisBlock)
        {
            throw new NotImplementedException();
        }

        public bool DoesChainExist(IKey key)
        {
            throw new NotImplementedException();
        }

        public BlockBase Get(long key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BlockBase> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BlockBase> GetAllByKey(IKey key)
        {
            throw new NotImplementedException();
        }

        public List<BlockBase> GetAllLastBlocksByType(ushort blockType)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetAllLastBlocksByType<T>() where T : BlockBase
        {
            throw new NotImplementedException();
        }

        public BlockBase GetBlockByOrder(IKey key, uint order)
        {
            throw new NotImplementedException();
        }

        public GenesisBlockBase GetGenesisBlock(IKey key)
        {
            throw new NotImplementedException();
        }

        public BlockBase GetLastBlock(IKey key)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void Update(long key, BlockBase item)
        {
            throw new NotImplementedException();
        }
    }
}
