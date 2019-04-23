using System;
using System.Buffers.Binary;
using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;
using Wist.Blockchain.Core.Exceptions;
using Wist.Core.Models;
using Wist.Core;

namespace Wist.Blockchain.Core.Parsers.Registry
{
    [RegisterExtension(typeof(IBlockParser), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryRegisterBlockParser : SignedBlockParserBase
    {
        public RegistryRegisterBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Registry_Register;

        public override PacketType PacketType => PacketType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if (version == 1)
            {
				int readBytes = 0;

                PacketType referencedPacketType = (PacketType)BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span);
				readBytes += sizeof(ushort);

                ushort referencedBlockType = BinaryPrimitives.ReadUInt16LittleEndian(spanBody.Span.Slice(readBytes));
				readBytes += sizeof(ushort);

                byte[] referencedBlockHash = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
				readBytes += Globals.DEFAULT_HASH_SIZE;

				byte[] referencedTarget = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
				readBytes += Globals.DEFAULT_HASH_SIZE;

				byte[] transactionKey = null;

				if((referencedBlockType & BlockTypes.TransitionalFlag) == BlockTypes.TransitionalFlag)
				{
					transactionKey = spanBody.Slice(readBytes, Globals.DEFAULT_HASH_SIZE).ToArray();
					readBytes += Globals.DEFAULT_HASH_SIZE;
				}

				RegistryRegisterBlock transactionRegisterBlock = new RegistryRegisterBlock
				{
					ReferencedPacketType = referencedPacketType,
					ReferencedBlockType = referencedBlockType,
					ReferencedBodyHash = referencedBlockHash,
					ReferencedTarget = referencedTarget,
					ReferencedTransactionKey = transactionKey
                };

                syncedBlockBase = transactionRegisterBlock;

                return spanBody.Slice(readBytes);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
