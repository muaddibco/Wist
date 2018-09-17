using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Parsers;
using Wist.BlockLattice.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.Mappers.Transactional
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalGenesisToBlockMapper : TransactionalMapperBase<TransactionalGenesis, TransactionalGenesisBlock>
    {
        public TransactionalGenesisToBlockMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository) 
            : base(blockParsersFactoriesRepository)
        {
        }

        public override TransactionalGenesisBlock Translate(TransactionalGenesis transactionalGenesis)
        {
            TransactionalGenesisBlock transactionalBlockBase = null;

            IBlockParser blockParser = null;

            blockParser = _blockParsersRepository.GetInstance(BlockTypes.Transaction_Genesis);

            transactionalBlockBase = (TransactionalGenesisBlock)blockParser.ParseBody(transactionalGenesis.BlockContent);

            return transactionalBlockBase;
        }
    }
}
