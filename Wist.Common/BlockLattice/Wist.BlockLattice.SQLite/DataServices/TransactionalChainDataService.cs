using System;
using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Translators;
using Wist.Core.Identity;
using Wist.BlockLattice.Core.Serializers;
using Wist.BlockLattice.SQLite.DataAccess;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDataService : IChainDataService
    {
        private readonly ITranslatorsRepository _mapperFactory;
        private readonly ISignatureSupportSerializersFactory _serializersFactory;

        public PacketType ChainType => PacketType.TransactionalChain;


        public TransactionalChainDataService(ITranslatorsRepository mapperFactory, ISignatureSupportSerializersFactory serializersFactory)
        {
            _mapperFactory = mapperFactory;
            _serializersFactory = serializersFactory;
        }

        public void Add(BlockBase block)
        {
            if (block is TransactionalGenesisBlock transactionalGenesisBlock)
            {
                IKey key = transactionalGenesisBlock.Signer;
                if (!DoesChainExist(key))
                {
                    CreateGenesisBlock(transactionalGenesisBlock);
                }
            }
            else if(block is TransactionalBlockBase transactionalBlockBase)
            {
                IKey key = transactionalBlockBase.Signer;

                if(DoesChainExist(key))
                {
                    DataAccessService.Instance.AddTransactionalBlock(key, transactionalBlockBase.BlockType, transactionalBlockBase.BodyBytes);
                }
            }
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
                DataAccessService.Instance.CreateTransactionalGenesisBlock(transactionalGenesisBlock.Signer, transactionalGenesisBlock.Version, transactionalGenesisBlock.NonHeaderBytes);
            }
        }

        public bool DoesChainExist(IKey key)
        {
            return DataAccessService.Instance.IsGenesisBlockExists(key);
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
            TransactionalGenesis transactionalGenesis = DataAccessService.Instance.GetTransactionalGenesisBlock(key);

            ITranslator<TransactionalGenesis, GenesisBlockBase> mapper = _mapperFactory.GetInstance<TransactionalGenesis, GenesisBlockBase>();

            return mapper?.Translate(transactionalGenesis);
        }

        public BlockBase GetLastBlock(IKey key)
        {
            TransactionalBlock transactionalBlock = DataAccessService.Instance.GetLastTransactionalBlock(key);

            ITranslator<TransactionalBlock, TransactionalBlockBase> mapper = _mapperFactory.GetInstance<TransactionalBlock, TransactionalBlockBase>();

            BlockBase block = mapper?.Translate(transactionalBlock);

            return block;
        }

        public IEnumerable<T> GetAllLastBlocksByType<T>() where T : BlockBase
        {
            List<T> blocks = new List<T>();

            if (typeof(T) == typeof(TransactionalGenesisBlock))
            {
                foreach (TransactionalGenesis genesis in DataAccessService.Instance.GetAllGenesisBlocks())
                {
                    ITranslator<TransactionalGenesis, TransactionalGenesisBlock> translator = _mapperFactory.GetInstance<TransactionalGenesis, TransactionalGenesisBlock>();

                    TransactionalGenesisBlock block = translator?.Translate(genesis);

                    blocks.Add(block as T);
                }
            }

            if(typeof(T) == typeof(TransactionalBlockBase))
            {
                foreach (TransactionalGenesis genesis in DataAccessService.Instance.GetAllGenesisBlocks())
                {
                    TransactionalBlock transactionalBlock = DataAccessService.Instance.GetLastTransactionalBlock(genesis.Identity);

                    ITranslator<TransactionalBlock, TransactionalBlockBase> translator = _mapperFactory.GetInstance<TransactionalBlock, TransactionalBlockBase>();

                    TransactionalBlockBase block = translator?.Translate(transactionalBlock);

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
                foreach (TransactionalGenesis genesis in DataAccessService.Instance.GetAllGenesisBlocks())
                {
                    ITranslator<TransactionalGenesis, TransactionalGenesisBlock> translator = _mapperFactory.GetInstance<TransactionalGenesis, TransactionalGenesisBlock>();

                    BlockBase block = translator?.Translate(genesis);

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

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public BlockBase Get(long key)
        {
            throw new NotImplementedException();
        }

        public void Update(long key, BlockBase item)
        {
            throw new NotImplementedException();
        }
    }
}
