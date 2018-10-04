using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Parsers.Synchronization
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducingBlockParser : SynchronizationBlockParserBase
    {
        public SynchronizationProducingBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationRepository) 
            : base(identityKeyProvidersRegistry, hashCalculationRepository)
        {
        }

        public override ushort BlockType => BlockTypes.Synchronization_TimeSyncProducingBlock;

        protected override Memory<byte> ParseSynchronization(ushort version, Memory<byte> spanBody, out SynchronizationBlockBase synchronizationBlockBase)
        {
            if (version == 1)
            {
                ushort round = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);

                SynchronizationProducingBlock synchronizationBlock = new SynchronizationProducingBlock
                {
                    Round = round
                };

                synchronizationBlockBase = synchronizationBlock;
                return spanBody.Slice(2);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
