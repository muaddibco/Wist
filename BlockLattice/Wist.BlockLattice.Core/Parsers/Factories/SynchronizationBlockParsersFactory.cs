using System;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationBlockParsersFactory : BlockParsersRepositoryBase
    {
        public SynchronizationBlockParsersFactory(IBlockParser[] blockParsers) : base(blockParsers)
        {
        }

        public override PacketType PacketType => PacketType.Synchronization;
    }
}
