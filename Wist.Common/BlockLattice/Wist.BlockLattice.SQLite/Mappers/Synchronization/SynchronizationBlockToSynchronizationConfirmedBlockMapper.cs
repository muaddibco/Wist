using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Parsers;
using Wist.BlockLattice.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.Mappers.Synchronization
{

    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationBlockToSynchronizationConfirmedBlockMapper : TranslatorBase<SynchronizationBlock, SynchronizationConfirmedBlock>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public SynchronizationBlockToSynchronizationConfirmedBlockMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Synchronization);
        }

        public override SynchronizationConfirmedBlock Translate(SynchronizationBlock synchronizationBlock)
        {
            if(synchronizationBlock == null)
            {
                return null;
            }

            SynchronizationConfirmedBlock synchronizationConfirmedBlock = null;

            IBlockParser blockParser = _blockParsersRepository.GetInstance(BlockTypes.Synchronization_ConfirmedBlock);

            synchronizationConfirmedBlock = blockParser.ParseBody(synchronizationBlock.BlockContent) as SynchronizationConfirmedBlock;

            return synchronizationConfirmedBlock;
        }
    }
}
