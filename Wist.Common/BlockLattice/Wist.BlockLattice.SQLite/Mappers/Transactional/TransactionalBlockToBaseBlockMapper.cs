using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Parsers;
using Wist.BlockLattice.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.Mappers.Transactional
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockToBaseBlockMapper : TransactionalMapperBase<TransactionalBlock, TransactionalBlockBase>
    {
        public TransactionalBlockToBaseBlockMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
            : base(blockParsersFactoriesRepository)
        {
        }

        public override TransactionalBlockBase Translate(TransactionalBlock transactionalBlock)
        {
            TransactionalBlockBase transactionalBlockBase = null;

            ushort blockType = transactionalBlock.BlockType;

            IBlockParser blockParser = null;

            blockParser = _blockParsersRepository.GetInstance(blockType);

            transactionalBlockBase = (TransactionalBlockBase)blockParser.ParseBody(transactionalBlock.BlockContent);

            return transactionalBlockBase;
        }
    }
}
