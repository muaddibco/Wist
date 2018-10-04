using System;
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
    public class RegistryConfirmationBlockParser : SyncedBlockParserBase
    {
        public RegistryConfirmationBlockParser(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IHashCalculationsRepository hashCalculationRepository) 
            : base(identityKeyProvidersRegistry, hashCalculationRepository)
        {
        }

        public override ushort BlockType => BlockTypes.Registry_ConfirmationBlock;

        public override PacketType PacketType => PacketType.Registry;

        protected override Memory<byte> ParseSynced(ushort version, Memory<byte> spanBody, out SyncedBlockBase syncedBlockBase)
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
