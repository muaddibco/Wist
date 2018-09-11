using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;
using Wist.Core.HashCalculations;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SignedBlockParserBase : BlockParserBase
    {
        protected readonly IIdentityKeyProvider _signerIdentityKeyProvider;

        public SignedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
            _signerIdentityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        protected override BlockBase ParseBlockBase(ushort version, Span<byte> spanBody)
        {
            SignedBlockBase signedBlockBase;
            Span<byte> spanPostBody = ParseSigned(version, spanBody, out signedBlockBase);

            signedBlockBase.Signature = spanPostBody.Slice(0, Globals.SIGNATURE_SIZE).ToArray();
            signedBlockBase.Signer = _signerIdentityKeyProvider.GetKey(spanPostBody.Slice(Globals.SIGNATURE_SIZE, Globals.NODE_PUBLIC_KEY_SIZE).ToArray());

            return signedBlockBase;
        }

        protected override byte[] GetBodyBytes(Span<byte> spanBody)
        {
            return spanBody.Slice(0, spanBody.Length - Globals.SIGNATURE_SIZE - Globals.NODE_PUBLIC_KEY_SIZE).ToArray();
        }

        protected abstract Span<byte> ParseSigned(ushort version, Span<byte> spanBody, out SignedBlockBase signedBlockBase);
    }
}
