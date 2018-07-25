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

        protected override BlockBase ParseBlockBase(ushort version, Span<byte> spanBody)
        {
            SignedBlockBase signedBlockBase;
            Span<byte> spanPostBody = ParseSigned(version, spanBody, out signedBlockBase);

            signedBlockBase.Key = _identityKeyProvider.GetKey(spanPostBody.Slice(0, Globals.NODE_PUBLIC_KEY_SIZE).ToArray());
            signedBlockBase.Signature = spanPostBody.Slice(Globals.NODE_PUBLIC_KEY_SIZE, Globals.SIGNATURE_SIZE).ToArray();

            return signedBlockBase;
        }

        protected abstract Span<byte> ParseSigned(ushort version, Span<byte> spanBody, out SignedBlockBase signedBlockBase);
    }
}
