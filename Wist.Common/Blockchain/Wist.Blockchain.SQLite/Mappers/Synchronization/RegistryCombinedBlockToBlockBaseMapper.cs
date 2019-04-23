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
    public class RegistryCombinedBlockToBlockBaseMapper : TranslatorBase<RegistryCombinedBlock, PacketBase>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public RegistryCombinedBlockToBlockBaseMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Synchronization);
        }

        public override PacketBase Translate(RegistryCombinedBlock registryCombinedBlock)
        {
            if (registryCombinedBlock == null)
            {
                return null;
            }

            SynchronizationRegistryCombinedBlock block = null;
            IBlockParser blockParser = _blockParsersRepository.GetInstance(BlockTypes.Synchronization_RegistryCombinationBlock);

            block = blockParser.Parse(registryCombinedBlock.Content) as SynchronizationRegistryCombinedBlock;
            block.SyncBlockHeight = registryCombinedBlock.SyncBlockHeight;

            return block;
        }
    }
}
