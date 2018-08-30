using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Parsers;
using Wist.BlockLattice.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockToBaseBlockMapper : TranslatorBase<TransactionalBlock, BlockBase>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public TransactionalBlockToBaseBlockMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.TransactionalChain);
        }

        public override BlockBase Translate(TransactionalBlock transactionalBlock)
        {
            BlockBase transactionalBlockBase = null;

            ushort blockType = transactionalBlock.BlockType;

            IBlockParser blockParser = null;

            blockParser = _blockParsersRepository.GetInstance(blockType);

            transactionalBlockBase = blockParser.ParseBody(transactionalBlock.BlockContent);

            return transactionalBlockBase;
        }
    }
}
