using System;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Handlers;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Logging;
using Wist.Core.ProofOfWork;
using Wist.Core.States;

namespace Wist.Node.Core.Registry
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistryVerifier : SignaturedBasedPacketVerifierBase
    {
        public TransactionRegistryVerifier(IStatesRepository statesRepository, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository, ICryptoService cryptoService, ILoggerService loggerService) : base(statesRepository, proofOfWorkCalculationRepository, cryptoService, loggerService)
        {
        }

        public override PacketType PacketType => PacketType.Registry;

        protected override bool ValidatePackerAfterSignature(Span<byte> span, ulong syncBlockHeight, byte[] publicKey)
        {
            return true;
        }
    }
}
