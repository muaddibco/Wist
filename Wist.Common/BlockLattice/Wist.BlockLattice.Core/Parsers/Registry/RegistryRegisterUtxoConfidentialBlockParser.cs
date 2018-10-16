using System;
using System.Buffers.Binary;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.DataModel.UtxoConfidential;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Exceptions;
using Wist.BlockLattice.Core.Parsers.UtxoConfidential;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryRegisterUtxoConfidentialBlockParser : UtxoConfidentialParserBase
    {
        public RegistryRegisterUtxoConfidentialBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override PacketType PacketType => PacketType.Registry;

        public override ushort BlockType => BlockTypes.Registry_RegisterUtxoConfidential;

        protected override Memory<byte> ParseUtxoConfidential(ushort version, Memory<byte> spanBody, out UtxoConfidentialBase utxoConfidentialBase)
        {
            if(version == 1)
            {
                PacketType referencedPacketType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
                ushort referencedBlockType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(2));
                byte[] referencedBlockHash = spanBody.Slice(4, Globals.DEFAULT_HASH_SIZE).ToArray();
                byte[] referencedTarget = spanBody.Slice(4 + Globals.DEFAULT_HASH_SIZE, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
                RegistryRegisterUtxoConfidentialBlock registryRegisterUtxoConfidentialBlock = new RegistryRegisterUtxoConfidentialBlock
                {
                    ReferencedPacketType = referencedPacketType,
                    ReferencedBlockType = referencedBlockType,
                    ReferencedBodyHash = referencedBlockHash,
                    ReferencedTarget = referencedTarget
                };

                utxoConfidentialBase = registryRegisterUtxoConfidentialBlock;

                return spanBody.Slice(4 + Globals.DEFAULT_HASH_SIZE + Globals.NODE_PUBLIC_KEY_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
