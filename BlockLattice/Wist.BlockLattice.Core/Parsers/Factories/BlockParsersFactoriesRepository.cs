using System.Linq;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Parsers.Factories
{
    [RegisterDefaultImplementation(typeof(IBlockParsersRepositoriesRepository), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class BlockParsersFactoriesRepository : IBlockParsersRepositoriesRepository
    {
        private readonly IBlockParsersRepository[] _blockParsersFactories;

        public BlockParsersFactoriesRepository(IBlockParsersRepository[] blockParsersFactories)
        {
            _blockParsersFactories = blockParsersFactories;
        }

        public IBlockParsersRepository GetBlockParsersRepository(PacketType packetType)
        {
            return _blockParsersFactories.FirstOrDefault(f => f.PacketType == packetType);
        }
    }
}
