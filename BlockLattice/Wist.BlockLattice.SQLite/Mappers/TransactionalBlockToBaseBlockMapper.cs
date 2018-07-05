using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.DataModel;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.Mappers
{
    public class TransactionalBlockToBaseBlockMapper : TranslatorBase<TransactionalBlock, BlockBase>
    {
        private readonly IBlockParsersFactory _blockParsersFactory;

        public TransactionalBlockToBaseBlockMapper(IBlockParsersFactoriesRepository blockParsersFactoriesRepository)
        {
            _blockParsersFactory = blockParsersFactoriesRepository.GetBlockParsersFactory(PacketType.TransactionalChain);
        }

        public override BlockBase Translate(TransactionalBlock transactionalBlock)
        {
            BlockBase transactionalBlockBase = null;

            ushort blockType = transactionalBlock.BlockType;

            IBlockParser blockParser = null;

            try
            {
                blockParser = _blockParsersFactory.Create(blockType);

                transactionalBlockBase = blockParser.ParseBody(transactionalBlock.BlockContent);
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
