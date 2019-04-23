using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Parsers;
using Wist.Core.Translators;

namespace Wist.Blockchain.SQLite.Mappers.UtxoConfidential
{
	public abstract class UtxoConfidentialMapperBase<TFrom, TTo> : TranslatorBase<TFrom, TTo>
	{
        protected readonly IBlockParsersRepository _blockParsersRepository;
		public UtxoConfidentialMapperBase(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository)
		{
			_blockParsersRepository = blockParsersFactoriesRepository.GetBlockParsersRepository(PacketType.UtxoConfidential);
		}
	}
}
