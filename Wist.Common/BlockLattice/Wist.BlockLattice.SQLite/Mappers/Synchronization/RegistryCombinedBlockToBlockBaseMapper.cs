using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Parsers;
using Wist.BlockLattice.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Translators;

namespace Wist.BlockLattice.SQLite.Mappers.Synchronization
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryCombinedBlockToBlockBaseMapper : TranslatorBase<RegistryCombinedBlock, BlockBase>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public RegistryCombinedBlockToBlockBaseMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Synchronization);
        }

        public override BlockBase Translate(RegistryCombinedBlock registryCombinedBlock)
        {
            if (registryCombinedBlock == null)
            {
                return null;
            }

            SynchronizationRegistryCombinedBlock block = null;
            IBlockParser blockParser = _blockParsersRepository.GetInstance(BlockTypes.Synchronization_ConfirmedBlock);

            block = blockParser.ParseBody(registryCombinedBlock.Content) as SynchronizationRegistryCombinedBlock;

            return block;
        }
    }
}
