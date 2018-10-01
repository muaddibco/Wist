using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryConfidenceBlockParser : SyncedBlockParserBase
    {
        public RegistryConfidenceBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationRepository) : base(identityKeyProvidersRegistry, hashCalculationRepository)
        {
        }

        public override ushort BlockType => BlockTypes.Registry_ConfidenceBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Span<byte> ParseSynced(ushort version, Span<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            if (version == 1)
            {
                RegistryConfidenceBlock registryConfidenceBlock = new RegistryConfidenceBlock();
                ushort bitMaskLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody);
                registryConfidenceBlock.BitMask = spanBody.Slice(2, bitMaskLength).ToArray();
                registryConfidenceBlock.ConfidenceProof = spanBody.Slice(2 + bitMaskLength, Globals.TRANSACTION_KEY_HASH_SIZE).ToArray();
                registryConfidenceBlock.ReferencedBlockHash = spanBody.Slice(2 + bitMaskLength + Globals.TRANSACTION_KEY_HASH_SIZE, Globals.DEFAULT_HASH_SIZE).ToArray();

                syncedBlockBase = registryConfidenceBlock;

                return spanBody.Slice(2 + bitMaskLength + Globals.TRANSACTION_KEY_HASH_SIZE + Globals.DEFAULT_HASH_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
