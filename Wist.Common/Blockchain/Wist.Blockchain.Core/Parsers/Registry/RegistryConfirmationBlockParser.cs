using System;
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
    public class RegistryConfirmationBlockParser : SignedBlockParserBase
    {
        public RegistryConfirmationBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        public override ushort BlockType => BlockTypes.Registry_ConfirmationBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedPacketBase syncedBlockBase)
        {
            if(version == 1)
            {
                RegistryConfirmationBlock block = new RegistryConfirmationBlock
                {
                    ReferencedBlockHash = spanBody.Slice(0, Globals.DEFAULT_HASH_SIZE).ToArray()
                };

                syncedBlockBase = block;

                return spanBody.Slice(Globals.DEFAULT_HASH_SIZE);
            }

            throw new BlockVersionNotSupportedException(version, BlockType);
        }
    }
}
