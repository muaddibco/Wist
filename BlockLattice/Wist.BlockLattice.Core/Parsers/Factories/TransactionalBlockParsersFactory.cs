﻿using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.BlockLattice.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersFactory), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockParsersFactory : BlockParsersFactoryBase
    {
        public TransactionalBlockParsersFactory(IBlockParser[] blockParsers) : base(blockParsers)
        {
        }

        public override PacketType PacketType => PacketType.TransactionalChain;
    }
}
