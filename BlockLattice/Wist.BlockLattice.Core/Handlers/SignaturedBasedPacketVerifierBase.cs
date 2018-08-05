using System;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Logging;
using Wist.Core.ProofOfWork;
using Wist.Core.States;

namespace Wist.BlockLattice.Core.Handlers
{
    public abstract class SignaturedBasedPacketVerifierBase : PowBasedPacketVerifierBase
    {
        private readonly ICryptoService _cryptoService;

        public SignaturedBasedPacketVerifierBase(IStatesRepository statesRepository, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository, ICryptoService cryptoService, ILoggerService loggerService) 
            : base(statesRepository, loggerService, proofOfWorkCalculationRepository)
        {
            _cryptoService = cryptoService;
        }

        protected override bool ValidatePacketAfterPow(Span<byte> span, ulong syncBlockHeight)
        {
            int actualMessageBodyLength = span.Length - (Globals.SIGNATURE_SIZE + Globals.NODE_PUBLIC_KEY_SIZE);

            byte[] messageBody = span.Slice(0, actualMessageBodyLength).ToArray();
            byte[] signature = span.Slice(actualMessageBodyLength, Globals.SIGNATURE_SIZE).ToArray();
            byte[] publickKey = span.Slice(actualMessageBodyLength + Globals.SIGNATURE_SIZE, Globals.NODE_PUBLIC_KEY_SIZE).ToArray();

            if (!VerifySignature(messageBody, signature, publickKey))
            {
                _log.Error("Signature is invalid");
                return false;
            }

            bool res = ValidatePackerAfterSignature(span, syncBlockHeight, publickKey);

            return true;
        }

        private bool VerifySignature(byte[] messageBody, byte[] signature, byte[] publicKey)
        {
            bool res = _cryptoService.Verify(signature, messageBody, publicKey);
            return res;
        }

        protected abstract bool ValidatePackerAfterSignature(Span<byte> span, ulong syncBlockHeight, byte[] publicKey);
    }
}
