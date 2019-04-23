using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.Blockchain.Core.Parsers.Synchronization
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationRegistryCombinedBlockParser : SynchronizationBlockParserBase
    {
        public SynchronizationRegistryCombinedBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Synchronization_RegistryCombinationBlock;

        protected override Memory<byte> ParseSynchronization(ushort version, Memory<byte> spanBody, out SynchronizationBlockBase synchronizationBlockBase)
        {
            SynchronizationRegistryCombinedBlock block = new SynchronizationRegistryCombinedBlock();

            if(version == 1)
            {
                ushort blockHashesCount = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                block.BlockHashes = new byte[blockHashesCount][];
                for (int i = 0; i < blockHashesCount; i++)
                {
                    block.BlockHashes[i] = spanBody.Slice(2 + i * Globals.DEFAULT_HASH_SIZE, Globals.DEFAULT_HASH_SIZE).ToArray();
                }

                synchronizationBlockBase = block;

                return spanBody.Slice(2 + blockHashesCount * Globals.DEFAULT_HASH_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
