using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.DataModel;
using Wist.Core.ExtensionMethods;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.Mappers
{
    public class TransactionalBlockToBaseBlockMapper : TranslatorBase<TransactionalBlock, BlockBase>
    {
        private readonly IBlockParsersFactory _blockParsersFactory;

        public TransactionalBlockToBaseBlockMapper(IBlockParsersFactory blockParsersFactory)
        {
            _blockParsersFactory = blockParsersFactory;
        }

        public override BlockBase Translate(TransactionalBlock transactionalBlock)
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
                            BlockHeight = transactionalBlock.BlockOrder
                        };
                        break;
                    case BlockTypes.Transaction_TransferFunds:
                        transactionalBlockBase = new TransferFundsBlockV1
                        {
                            BlockHeight = transactionalBlock.BlockOrder
                        };
                        break;
                    case BlockTypes.Transaction_Confirm:
                        break;
                    case BlockTypes.Transaction_Dpos:
                        transactionalBlockBase = new TransactionalDposVote
                        {
                            BlockHeight = transactionalBlock.BlockOrder
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
