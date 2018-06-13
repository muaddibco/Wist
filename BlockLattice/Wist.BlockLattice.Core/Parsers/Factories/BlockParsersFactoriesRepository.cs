using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;

namespace Wist.BlockLattice.Core.Parsers.Factories
{
    [RegisterDefaultImplementation(typeof(IBlockParsersFactoriesRepository), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class BlockParsersFactoriesRepository : IBlockParsersFactoriesRepository
    {
        private readonly IBlockParsersFactory[] _blockParsersFactories;

        public BlockParsersFactoriesRepository(IBlockParsersFactory[] blockParsersFactories)
        {
            _blockParsersFactories = blockParsersFactories;
        }

        public IBlockParsersFactory GetBlockParsersFactory(PacketType packetType)
        {
            return _blockParsersFactories.FirstOrDefault(f => f.ChainType == packetType);
        }
    }
}
