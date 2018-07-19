using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;
using Wist.Core.ProofOfWork;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SignedBlockParserBase : BlockParserBase
    {
        protected readonly IIdentityKeyProvider _identityKeyProvider;

        public SignedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base()
        {
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        protected override BlockBase Parse(ushort version, BinaryReader br)
        {
            SignedBlockBase signedBlockBase = ParseSigned(version, br);

            signedBlockBase.Key = _identityKeyProvider.GetKey(br.ReadBytes(Globals.NODE_PUBLIC_KEY_SIZE));
            signedBlockBase.Signature = br.ReadBytes(Globals.SIGNATURE_SIZE);

            return signedBlockBase;
        }

        protected abstract SignedBlockBase ParseSigned(ushort version, BinaryReader br);
    }
}
