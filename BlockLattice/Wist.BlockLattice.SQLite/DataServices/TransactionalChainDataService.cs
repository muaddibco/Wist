using System;
using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Mappers;
using Wist.Core.Identity;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDataService : IChainDataService
    {
        private readonly IMapperFactory _mapperFactory;

        public PacketType ChainType => PacketType.TransactionalChain;


        public TransactionalChainDataService(IMapperFactory mapperFactory)
        {
            _mapperFactory = mapperFactory;
        }

        public void Add(BlockBase block)
        {
            throw new NotImplementedException();
        }

        public void CreateGenesisBlock(GenesisBlockBase genesisBlock)
        {
            if (genesisBlock == null)
            {
                throw new ArgumentNullException(nameof(genesisBlock));
            }

            TransactionalGenesisBlockV1 transactionalGenesisBlock = genesisBlock as TransactionalGenesisBlockV1;

            if(transactionalGenesisBlock == null)
            {
                throw new ArgumentOutOfRangeException(nameof(genesisBlock));
            }

            LatticeDataService.Instance.CreateTransactionalGenesisBlock(transactionalGenesisBlock.OriginalHash);
        }

        public bool DoesChainExist(IKey key)
        {
            return LatticeDataService.Instance.IsGenesisBlockExists(key);
        }

        public BlockBase[] GetAllBlocks(IKey key)
        {
            throw new NotImplementedException();
        }

        public BlockBase GetBlockByOrder(IKey key, uint order)
        {
            throw new NotImplementedException();
        }

        public GenesisBlockBase GetGenesisBlock(IKey key)
        {
            TransactionalGenesis transactionalGenesis = LatticeDataService.Instance.GetTransactionalGenesisBlock(key.Value);

            return new TransactionalGenesisBlockV1
            {
                OriginalHash = transactionalGenesis.OriginalHash.HexStringToByteArray(),
                BlockHeight = 0
            };
        }

        public BlockBase GetLastBlock(IKey key)
        {
            TransactionalBlock transactionalBlock = LatticeDataService.Instance.GetLastTransactionalBlock(key.Value);

            IMapper<TransactionalBlock, BlockBase> mapper = _mapperFactory.GetMapper<TransactionalBlock, BlockBase>();

            BlockBase block = mapper?.Convert(transactionalBlock);

            return block;
        }

        public List<BlockBase> GetAllLastBlocksByType(ushort blockType)
        {
            List<BlockBase> blocks = new List<BlockBase>();

            foreach (TransactionalGenesis genesis in LatticeDataService.Instance.GetAllGenesisBlocks())
            {
                TransactionalBlock transactionalBlock = LatticeDataService.Instance.GetLastBlockModification(genesis.OriginalHash, blockType);

                IMapper<TransactionalBlock, BlockBase> mapper = _mapperFactory.GetMapper<TransactionalBlock, BlockBase>();

                BlockBase block = mapper?.Convert(transactionalBlock);

                blocks.Add(block);
            }

            return blocks;
        }

        public IEnumerable<BlockBase> GetAllByKey(IKey key)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<BlockBase> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
