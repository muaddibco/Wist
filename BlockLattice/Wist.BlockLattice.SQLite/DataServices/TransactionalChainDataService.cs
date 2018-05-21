using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.DataModel;
using Wist.BlockLattice.SQLite;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.BlockLattice.Core.DataModel;
using System.Linq;
using Wist.Core.Mappers;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDataService : IChainDataService
    {
        private readonly IMapperFactory _mapperFactory;

        public ChainType ChainType => ChainType.TransactionalChain;


        public TransactionalChainDataService(IMapperFactory mapperFactory)
        {
            _mapperFactory = mapperFactory;
        }

        public void AddBlock(BlockBase block)
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

        public bool DoesChainExist(byte[] key)
        {
            return LatticeDataService.Instance.IsGenesisBlockExists(key);
        }

        public BlockBase[] GetAllBlocks(byte[] key)
        {
            throw new NotImplementedException();
        }

        public BlockBase GetBlockByOrder(byte[] key, uint order)
        {
            throw new NotImplementedException();
        }

        public GenesisBlockBase GetGenesisBlock(byte[] key)
        {
            TransactionalGenesis transactionalGenesis = LatticeDataService.Instance.GetTransactionalGenesisBlock(key);

            return new TransactionalGenesisBlockV1
            {
                OriginalHash = transactionalGenesis.OriginalHash.HexStringToByteArray(),
                BlockOrder = 0
            };
        }

        public BlockBase GetLastBlock(byte[] key)
        {
            TransactionalBlock transactionalBlock = LatticeDataService.Instance.GetLastTransactionalBlock(key);

            IMapper<TransactionalBlock, BlockBase> mapper = _mapperFactory.GetMapper<TransactionalBlock, BlockBase>();

            BlockBase block = mapper?.Convert(transactionalBlock);

            return block;
        }

        public List<BlockBase> GetAllLastBlocksByType(ushort blockType)
        {
            List<BlockBase> blocks = new List<BlockBase>();

            LatticeDataService.Instance.GetAllGenesisBlocks().ForEach(g => 
            {
                TransactionalBlock transactionalBlock = LatticeDataService.Instance.GetLastBlockModification(g.OriginalHash, blockType);
            });

            return blocks;
        }
    }
}
