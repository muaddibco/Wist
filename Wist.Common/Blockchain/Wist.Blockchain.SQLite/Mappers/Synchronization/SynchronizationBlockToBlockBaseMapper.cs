using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Parsers;
using Wist.Blockchain.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;
using Wist.Core.Translators;

namespace Wist.Blockchain.SQLite.Mappers.Synchronization
{

    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationBlockToBlockBaseMapper : TranslatorBase<SynchronizationBlock, PacketBase>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public SynchronizationBlockToBlockBaseMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Synchronization);
        }

        public override PacketBase Translate(SynchronizationBlock synchronizationBlock)
        {
            if(synchronizationBlock == null)
            {
                return null;
            }

            SynchronizationConfirmedBlock synchronizationConfirmedBlock = null;

            IBlockParser blockParser = _blockParsersRepository.GetInstance(BlockTypes.Synchronization_ConfirmedBlock);

            synchronizationConfirmedBlock = blockParser.Parse(synchronizationBlock.BlockContent) as SynchronizationConfirmedBlock;

            return synchronizationConfirmedBlock;
        }
    }
}
