using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SyncedBlockParserBase : SignedBlockParserBase
    {
        public SyncedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) 
            : base(identityKeyProvidersRegistry)
        {
        }

        protected override SignedBlockBase ParseSigned(ushort version, BinaryReader br)
        {
            ulong blockHeight = br.ReadUInt64();
            byte[] prevHash = br.ReadBytes(Globals.HASH_SIZE);
            SyncedBlockBase syncedBlockBase = ParseSynced(version, br);
            syncedBlockBase.BlockHeight = blockHeight;
            syncedBlockBase.HashPrev = prevHash;

            return syncedBlockBase;
        }

        protected abstract SyncedBlockBase ParseSynced(ushort version, BinaryReader br);
    }
}
