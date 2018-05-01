using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel.Enums;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.DataModel;
using Wist.BlockLattice.MySql;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;

namespace Wist.BlockLattice.SQLite.DataServices
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalChainDataService : IChainDataService<TransactionalGenesisBlock, TransactionalBlockBase>
    {
        public ChainType ChainType => ChainType.TransactionalChain;

        private readonly IBlockParsersFactory _blockParsersFactory;

        public TransactionalChainDataService(IBlockParsersFactory blockParsersFactory)
        {
            _blockParsersFactory = blockParsersFactory;
        }

        public void AddBlock(TransactionalBlockBase block)
        {
            throw new NotImplementedException();
        }

        public void CreateGenesisBlock(TransactionalGenesisBlock genesisBlock)
        {
            LatticeDataService.Instance.CreateTransactionalGenesisBlock(genesisBlock.OriginalHash);
        }

        public bool DoesChainExist(byte[] key)
        {
            return LatticeDataService.Instance.IsGenesisBlockExists(key);
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
            TransactionalGenesis transactionalGenesis = LatticeDataService.Instance.GetTransactionalGenesisBlock(key);

            return new TransactionalGenesisBlock
            {
                OriginalHash = transactionalGenesis.OriginalHash.HexStringToByteArray(),
                BlockOrder = 0
            };
        }

        public TransactionalBlockBase GetLastBlock(byte[] key)
        {
            TransactionalBlock transactionalBlock = LatticeDataService.Instance.GetLastTransactionalBlock(key);
            TransactionalBlockBase transactionalBlockBase = null;

            BlockType blockType = (BlockType)transactionalBlock.BlockType;

            IBlockParser blockParser = null;

            try
            {
                blockParser = _blockParsersFactory.Create(blockType);

                switch (blockType)
                {
                    case BlockType.Transaction_AcceptFunds:
                        transactionalBlockBase = new AcceptFundsBlock
                        {
                            OriginalHash = transactionalBlock.TransactionalGenesis.OriginalHash.HexStringToByteArray(),
                            BlockOrder = transactionalBlock.BlockOrder
                        };
                        blockParser.FillBlockBody(transactionalBlockBase, transactionalBlock.BlockContent);
                        break;
                    case BlockType.Transaction_TransferFunds:
                        transactionalBlockBase = new TransferFundsBlock
                        {
                            OriginalHash = transactionalBlock.TransactionalGenesis.OriginalHash.HexStringToByteArray(),
                            BlockOrder = transactionalBlock.BlockOrder
                        };
                        blockParser.FillBlockBody(transactionalBlockBase, transactionalBlock.BlockContent);
                        break;
                    case BlockType.Transaction_Confirm:
                        break;
                    default:
                        break;
                }
            }
            finally
            {
                if (blockParser != null)
                {
                    _blockParsersFactory.Utilize(blockParser);
                }
            }

            return transactionalBlockBase;
        }
    }
}
