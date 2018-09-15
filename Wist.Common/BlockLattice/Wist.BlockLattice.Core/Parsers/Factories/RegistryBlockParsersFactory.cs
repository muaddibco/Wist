using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryBlockParsersFactory : BlockParsersRepositoryBase
    {
        public RegistryBlockParsersFactory(IBlockParser[] blockParsers) : base(blockParsers)
        {
        }

        public override PacketType PacketType => PacketType.Registry;
    }
}
