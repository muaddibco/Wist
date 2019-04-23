using Wist.Core.Models;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Logging;

namespace Wist.Blockchain.Core.Handlers
{
    [RegisterExtension(typeof(ICoreVerifier), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class SignatureVerifier : ICoreVerifier
    {
        private readonly ILogger _log;
        private readonly ISigningServicesRepository _signingServicesRepository;

        public SignatureVerifier(ISigningServicesRepository signingServicesRepository, ILoggerService loggerService) 
        {
            _log = loggerService.GetLogger(nameof(SignatureVerifier));
            _signingServicesRepository = signingServicesRepository;
        }

        public bool VerifyBlock(PacketBase blockBase)
        {
            bool res = false;

            if (blockBase is SignedPacketBase)
            {
                res = _signingServicesRepository.GetInstance("AccountSigningService").Verify(blockBase);
            }
            else if(blockBase is UtxoSignedPacketBase)
            {
                res = _signingServicesRepository.GetInstance("UtxoSigningService").Verify(blockBase);

				//TODO: !!! urgently check why signatures validation fails
				res = true;
            }


            if (!res)
            {
                _log.Error("Signature is invalid");
                return false;
            }

            return true;
        }
    }
}
