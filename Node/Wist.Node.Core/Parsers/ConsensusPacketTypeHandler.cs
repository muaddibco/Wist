using System;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Handlers;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.ProofOfWork;
using Wist.Core.States;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Parsers
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class ConsensusPacketTypeHandler : SignaturedBasedPacketVerifierBase
    {
        private readonly IConsensusHub _consensusHub;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public ConsensusPacketTypeHandler(IConsensusHub consensusHub, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, IStatesRepository statesRepository, IProofOfWorkCalculationRepository proofOfWorkCalculationRepository, ICryptoService cryptoService, ILoggerService loggerService)
            : base(statesRepository, proofOfWorkCalculationRepository, cryptoService, loggerService)
        {
            _consensusHub = consensusHub;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override PacketType PacketType => PacketType.Consensus;

        protected override bool ValidatePackerAfterSignature(Span<byte> span, ulong syncBlockHeight, byte[] publickKey)
        {
            if(!VerifyConsensusParticipant(publickKey))
            {
                _log.Error("Invalid consensus group participant");
                return false;
            }

            return true;
        }

        private bool VerifyConsensusParticipant(byte[] publicKey)
        {
            IKey key = _identityKeyProvider.GetKey(publicKey);

            return _consensusHub.GroupParticipants.ContainsKey(key);
        }
    }
}
