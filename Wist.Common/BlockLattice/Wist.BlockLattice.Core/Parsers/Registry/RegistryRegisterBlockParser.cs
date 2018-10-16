using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;
using Wist.BlockLattice.Core.Exceptions;

namespace Wist.BlockLattice.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryRegisterBlockParser : SyncedBlockParserBase
    {
        public RegistryRegisterBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationsRepository) 
            : base(identityKeyProvidersRegistry, hashCalculationsRepository)
        {
        }

        public override ushort BlockType => BlockTypes.Registry_Register;

        public override PacketType PacketType => PacketType.Registry;

        protected override Memory<byte> ParseSynced(ushort version, Memory<byte> spanBody, out SyncedBlockBase syncedBlockBase)
        {
            if (version == 1)
            {
                PacketType referencedPacketType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                ushort referencedBlockType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(2));
                byte[] referencedBlockHash = spanBody.Slice(4, Globals.DEFAULT_HASH_SIZE).ToArray();
                byte[] referencedTargetHash = spanBody.Slice(4 + Globals.DEFAULT_HASH_SIZE, Globals.DEFAULT_HASH_SIZE).ToArray();
                RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
                {
                    ReferencedPacketType = referencedPacketType,
                    ReferencedBlockType = referencedBlockType,
                    ReferencedBodyHash = referencedBlockHash,
                    ReferencedTarget = referencedTargetHash
                };

                syncedBlockBase = transactionRegisterBlock;

                return spanBody.Slice(4 + Globals.DEFAULT_HASH_SIZE + Globals.DEFAULT_HASH_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
