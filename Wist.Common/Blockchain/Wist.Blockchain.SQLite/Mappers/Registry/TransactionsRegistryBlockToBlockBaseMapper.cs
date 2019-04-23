using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Parsers;
using Wist.Blockchain.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;
using Wist.Core.Translators;

namespace Wist.Blockchain.SQLite.Mappers.Registry
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryBlockToBlockBaseMapper : TranslatorBase<TransactionsRegistryBlock, PacketBase>
    {
        private readonly IBlockParsersRepository _blockParsersRepository;

        public TransactionsRegistryBlockToBlockBaseMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
        {
            _blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.Registry);
        }

        public override PacketBase Translate(TransactionsRegistryBlock obj)
        {
            if(obj == null)
            {
                return null;
            }

            IBlockParser blockParser = _blockParsersRepository.GetInstance(BlockTypes.Registry_FullBlock);

            RegistryFullBlock registryFullBlock = (RegistryFullBlock)blockParser.Parse(obj.Content);

            return registryFullBlock;
        }
    }
}
