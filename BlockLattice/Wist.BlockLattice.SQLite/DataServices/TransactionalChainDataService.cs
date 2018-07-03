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
using Wist.Core.Translators;
using Wist.Core.Identity;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDataService : IChainDataService
    {
        private readonly ITranslatorsFactory _mapperFactory;
        private readonly ISignatureSupportSerializersFactory _serializersFactory;

        public PacketType ChainType => PacketType.TransactionalChain;


        public TransactionalChainDataService(ITranslatorsFactory mapperFactory, ISignatureSupportSerializersFactory serializersFactory)
        {
            _mapperFactory = mapperFactory;
            _serializersFactory = serializersFactory;
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

            TransactionalGenesisBlock transactionalGenesisBlock = genesisBlock as TransactionalGenesisBlock;

            if(transactionalGenesisBlock == null)
            {
                throw new ArgumentOutOfRangeException(nameof(genesisBlock));
            }

            using (ISignatureSupportSerializer serializer = _serializersFactory.Create(transactionalGenesisBlock))
            {
                LatticeDataService.Instance.CreateTransactionalGenesisBlock(transactionalGenesisBlock.Key, transactionalGenesisBlock.Version, serializer.GetBytes());
            }
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
            TransactionalGenesisModification transactionalGenesis = LatticeDataService.Instance.GetLastTransactionalGenesisModification(key);

            ITranslator<TransactionalGenesisModification, GenesisBlockBase> mapper = _mapperFactory.GetTranslator<TransactionalGenesisModification, GenesisBlockBase>();

            return mapper?.Translate(transactionalGenesis);
        }

        public BlockBase GetLastBlock(IKey key)
        {
            TransactionalBlock transactionalBlock = LatticeDataService.Instance.GetLastTransactionalBlock(key);

            ITranslator<TransactionalBlock, BlockBase> mapper = _mapperFactory.GetTranslator<TransactionalBlock, BlockBase>();

            BlockBase block = mapper?.Translate(transactionalBlock);

            return block;
        }

        public IEnumerable<T> GetAllLastBlocksByType<T>() where T : BlockBase
        {
            List<T> blocks = new List<T>();

            if (typeof(T) == typeof(TransactionalGenesisBlock))
            {
                foreach (TransactionalGenesis genesis in LatticeDataService.Instance.GetAllGenesisBlocks())
                {
                    TransactionalGenesisModification transactionalBlock = LatticeDataService.Instance.GetLastTransactionalGenesisModification(genesis);

                    ITranslator<TransactionalGenesisModification, TransactionalGenesisBlock> translator = _mapperFactory.GetTranslator<TransactionalGenesisModification, TransactionalGenesisBlock>();

                    TransactionalGenesisBlock block = translator?.Translate(transactionalBlock);

                    blocks.Add(block as T);
                }
            }

            if(typeof(T) == typeof(TransactionalBlockBaseV1))
            {
                foreach (TransactionalGenesis genesis in LatticeDataService.Instance.GetAllGenesisBlocks())
                {
                    TransactionalBlock transactionalBlock = LatticeDataService.Instance.GetLastTransactionalBlock(genesis.Identity, false);

                    ITranslator<TransactionalBlock, TransactionalBlockBaseV1> translator = _mapperFactory.GetTranslator<TransactionalBlock, TransactionalBlockBaseV1>();

                    TransactionalBlockBaseV1 block = translator?.Translate(transactionalBlock);

                    blocks.Add(block as T);
                }
            }

            return blocks;
        }

        public List<BlockBase> GetAllLastBlocksByType(ushort blockType)
        {
            List<BlockBase> blocks = new List<BlockBase>();

            if(blockType == BlockTypes.Transaction_Genesis)
            {
                foreach (TransactionalGenesis genesis in LatticeDataService.Instance.GetAllGenesisBlocks())
                {
                    TransactionalGenesisModification transactionalBlock = LatticeDataService.Instance.GetLastTransactionalGenesisModification(genesis);

                    ITranslator<TransactionalGenesisModification, BlockBase> translator = _mapperFactory.GetTranslator<TransactionalGenesisModification, BlockBase>();

                    BlockBase block = translator?.Translate(transactionalBlock);

                    blocks.Add(block);
                }
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
