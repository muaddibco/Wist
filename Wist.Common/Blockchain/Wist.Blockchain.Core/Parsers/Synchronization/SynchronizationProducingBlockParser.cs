using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Synchronization
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationProducingBlockParser : SynchronizationBlockParserBase
    {
        public SynchronizationProducingBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
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
