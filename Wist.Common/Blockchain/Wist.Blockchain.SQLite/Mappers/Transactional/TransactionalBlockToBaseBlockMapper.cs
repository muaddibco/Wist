using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.Parsers;
using Wist.Blockchain.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;
using Wist.Core.Translators;

namespace Wist.Blockchain.SQLite.Mappers.Transactional
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockToBaseBlockMapper : TransactionalMapperBase<TransactionalBlock, PacketBase>
    {
        public TransactionalBlockToBaseBlockMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
            : base(blockParsersFactoriesRepository)
        {
        }

        public override PacketBase Translate(TransactionalBlock transactionalBlock)
        {
            if(transactionalBlock == null)
            {
                return null;
            }

            PacketBase transactionalBlockBase = null;

            ushort blockType = transactionalBlock.BlockType;

            IBlockParser blockParser = null;

            blockParser = _blockParsersRepository.GetInstance(blockType);

            transactionalBlockBase = blockParser.Parse(transactionalBlock.BlockContent);

            return transactionalBlockBase;
        }
    }
}
