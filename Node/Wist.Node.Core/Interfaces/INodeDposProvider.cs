using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Node.Core.Interfaces
{
    [ExtensionPoint]
    public interface INodeDposProvider
    {
        PacketType PacketType { get; }

        double GetVotesForCandidate(IKey candidateKey);

        int GetCandidateRating(IKey candidateKey);

        void UpdateContribution(BlockBase block);

        void Initialize();
    }
}
