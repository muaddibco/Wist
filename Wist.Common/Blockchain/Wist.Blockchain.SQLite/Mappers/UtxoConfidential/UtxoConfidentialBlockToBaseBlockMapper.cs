using Wist.Blockchain.Core.Parsers;
using Wist.Blockchain.DataModel;
using Wist.Blockchain.SQLite.Mappers.Transactional;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Models;
using Wist.Core.Translators;

namespace Wist.Blockchain.SQLite.Mappers.UtxoConfidential
{
	[RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
	public class UtxoConfidentialBlockToBaseBlockMapper : UtxoConfidentialMapperBase<UtxoConfidentialBlock, PacketBase>
	{
		public UtxoConfidentialBlockToBaseBlockMapper(IBlockParsersRepositoriesRepository blockParsersFactoriesRepository) 
			: base(blockParsersFactoriesRepository)
		{
		}

		public override PacketBase Translate(UtxoConfidentialBlock block)
		{
			PacketBase packetBase = null;

			ushort blockType = block.BlockType;

			IBlockParser blockParser = null;

			blockParser = _blockParsersRepository.GetInstance(blockType);

			packetBase = blockParser.Parse(block.BlockContent);

			return packetBase;

		}
	}
}
