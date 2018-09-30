using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Node.Core.Rating
{
    [ExtensionPoint]
    public interface INodesRatingProvider
    {
        PacketType PacketType { get; }

        double GetVotesForCandidate(IKey candidateKey);

        int GetCandidateRating(IKey candidateKey);

        bool IsCandidateInTopList(IKey candidateKey);

        int GetParticipantsCount();

        void UpdateContribution(BlockBase block);

        void Initialize();
    }
}
