using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.Parsers
{
    [RegisterExtension(typeof(IPacketVerifier), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusPacketVerifier : IPacketVerifier
    {
        private readonly IConsensusHub _consensusHub;
        private readonly ILogger _log;

        public ConsensusPacketVerifier(IConsensusHub consensusHub, ILoggerService loggerService)
        {
            _consensusHub = consensusHub;
            _log = loggerService.GetLogger(nameof(ConsensusPacketVerifier));
        }

        public PacketType PacketType => PacketType.Consensus;

        public bool ValidatePacket(BlockBase blockBase)
        {
            SignedBlockBase signedBlockBase = (SignedBlockBase)blockBase;
            if(!VerifyConsensusParticipant(signedBlockBase.Key))
            {
                _log.Error("Invalid consensus group participant");
                return false;
            }

            return true;
        }

        private bool VerifyConsensusParticipant(IKey key)
        {
            return _consensusHub.GroupParticipants.ContainsKey(key);
        }
    }
}
