using Wist.BlockLattice.Core.DataModel;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Logging;

namespace Wist.BlockLattice.Core.Handlers
{
    [RegisterExtension(typeof(ICoreVerifier), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SignatureVerifier : ICoreVerifier
    {
        private readonly ICryptoService _cryptoService;
        private readonly ILogger _log;

        public SignatureVerifier(ICryptoService cryptoService, ILoggerService loggerService) 
        {
            _cryptoService = cryptoService;
            _log = loggerService.GetLogger(nameof(SignatureVerifier));
        }

        public bool VerifyBlock(BlockBase blockBase)
        {
            SignedBlockBase signedBlockBase = (SignedBlockBase)blockBase;

            byte[] messageBody = signedBlockBase.BodyBytes.ToArray();
            byte[] signature = signedBlockBase.Signature.ToArray();
            byte[] publickKey = signedBlockBase.Signer.Value.ToArray();

            if (!VerifySignature(messageBody, signature, publickKey))
            {
                _log.Error("Signature is invalid");
                return false;
            }

            return true;
        }

        private bool VerifySignature(byte[] messageBody, byte[] signature, byte[] publicKey)
        {
            bool res = _cryptoService.Verify(signature, messageBody, publicKey);
            return res;
        }
    }
}
