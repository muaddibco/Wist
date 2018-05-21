using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.DataModel;
using Wist.Core.ExtensionMethods;
using Wist.Core.Mappers;

namespace Wist.BlockLattice.SQLite.Mappers
{
    public class TransactionalBlockToBaseBlockMapper : MapperBase<TransactionalBlock, BlockBase>
    {
        private readonly IBlockParsersFactory _blockParsersFactory;

        public TransactionalBlockToBaseBlockMapper(IBlockParsersFactory blockParsersFactory)
        {
            _blockParsersFactory = blockParsersFactory;
        }

        public override BlockBase Convert(TransactionalBlock transactionalBlock)
        {
            TransactionalBlockBase transactionalBlockBase = null;

            ushort blockType = transactionalBlock.BlockType;

            IBlockParser blockParser = null;

            try
            {
                blockParser = _blockParsersFactory.Create(blockType);

                switch (blockType)
                {
                    case BlockTypes.Transaction_AcceptFunds:
                        transactionalBlockBase = new AcceptFundsBlockV1
                        {
                            OriginalHash = transactionalBlock.TransactionalGenesis.OriginalHash.HexStringToByteArray(),
                            BlockOrder = transactionalBlock.BlockOrder
                        };
                        break;
                    case BlockTypes.Transaction_TransferFunds:
                        transactionalBlockBase = new TransferFundsBlockV1
                        {
                            OriginalHash = transactionalBlock.TransactionalGenesis.OriginalHash.HexStringToByteArray(),
                            BlockOrder = transactionalBlock.BlockOrder
                        };
                        break;
                    case BlockTypes.Transaction_Confirm:
                        break;
                    case BlockTypes.Transaction_Dpos:
                        transactionalBlockBase = new TransactionalDposVote
                        {
                            OriginalHash = transactionalBlock.TransactionalGenesis.OriginalHash.HexStringToByteArray(),
                            BlockOrder = transactionalBlock.BlockOrder
                        };
                        break;
                    default:
                        break;
                }
                blockParser.FillBlockBody(transactionalBlockBase, transactionalBlock.BlockContent);
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
