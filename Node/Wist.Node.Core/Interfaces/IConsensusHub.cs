using System;
using System.Collections.Generic;
using System.Text;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Interfaces
{
    public interface IConsensusHub
    {
        // TODO: weigh necessity to make map PublicKey -> ConsensusGroupParticipant
        ConsensusGroupParticipant[] GroupParticipants { get; }
    }
}
