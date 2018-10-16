using System;
using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Identity;

namespace Wist.BlockLattice.Core.Parsers
{
    public abstract class SignedBlockParserBase : BlockParserBase
    {
        protected readonly IIdentityKeyProvider _signerIdentityKeyProvider;

        public SignedBlockParserBase(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry) : base(identityKeyProvidersRegistry)
        {
            _signerIdentityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        protected override BlockBase ParseBlockBase(ushort version, Memory<byte> spanBody, out Memory<byte> spanPostBody)
        {
            spanPostBody = ParseSigned(version, spanBody, out SignedBlockBase signedBlockBase);

            signedBlockBase.Signature = spanPostBody.Slice(0, Globals.SIGNATURE_SIZE);
            signedBlockBase.Signer = _signerIdentityKeyProvider.GetKey(spanPostBody.Slice(Globals.SIGNATURE_SIZE, Globals.NODE_PUBLIC_KEY_SIZE));

            spanPostBody = spanPostBody.Slice(Globals.SIGNATURE_SIZE + Globals.NODE_PUBLIC_KEY_SIZE);
            return signedBlockBase;
        }

        protected override Memory<byte> GetBodyBytes(Memory<byte> spanBody)
        {
            return spanBody.Slice(0, spanBody.Length - Globals.SIGNATURE_SIZE - Globals.NODE_PUBLIC_KEY_SIZE);
        }

        protected abstract Memory<byte> ParseSigned(ushort version, Memory<byte> spanBody, out SignedBlockBase signedBlockBase);
    }
}
