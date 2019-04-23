using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Models;

namespace Wist.Blockchain.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryConfidenceBlockParser : SignedBlockParserBase
    {
        public RegistryConfidenceBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Registry_ConfidenceBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if (version == 1)
            {
                RegistryConfidenceBlock registryConfidenceBlock = new RegistryConfidenceBlock();
                ushort bitMaskLength = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
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
