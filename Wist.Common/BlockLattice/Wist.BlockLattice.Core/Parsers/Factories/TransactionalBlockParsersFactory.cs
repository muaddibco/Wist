using System;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers.Factories
{
    [RegisterExtension(typeof(IBlockParsersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalBlockParsersFactory : BlockParsersRepositoryBase
    {
        public TransactionalBlockParsersFactory(IBlockParser[] blockParsers, IHashCalculationsRepository proofOfWorkCalculationRepository) : base(blockParsers)
        {
        }

        public override PacketType PacketType => PacketType.Transactional;
    }
}
