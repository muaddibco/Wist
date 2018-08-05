using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Identity;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Consensus
{
    [RegisterDefaultImplementation(typeof(IConsensusHub), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class ConsensusHub : IConsensusHub
    {
        public Dictionary<IKey, ConsensusGroupParticipant> GroupParticipants { get; } = new Dictionary<IKey, ConsensusGroupParticipant>();

        public int TotalWeight => throw new NotImplementedException();
    }
}
