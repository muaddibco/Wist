﻿using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Blockchain.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class UtxoConfidentialBlockParsersFactory : BlockParsersRepositoryBase
    {
        public UtxoConfidentialBlockParsersFactory(IBlockParser[] blockParsers) : base(blockParsers)
        {
        }

        public override PacketType PacketType => PacketType.UtxoConfidential;
    }
}
