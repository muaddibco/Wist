using Wist.Blockchain.Core.DataModel;
using Wist.Blockchain.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Identity;
using Wist.Core.Models;

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

        void UpdateContribution(PacketBase block);

        void Initialize();
    }
}
