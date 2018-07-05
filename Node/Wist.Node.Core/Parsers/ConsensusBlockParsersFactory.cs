using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Parsers.Factories;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Node.Core.Parsers
{
    [RegisterExtension(typeof(IBlockParsersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusBlockParsersFactory : BlockParsersFactoryBase
    {
        public ConsensusBlockParsersFactory(IBlockParser[] blockParsers) : base(blockParsers)
        {
        }

        public override PacketType PacketType => PacketType.Consensus;
    }
}
