using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Identity;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Interfaces
{
    [ServiceContract]
    public interface IConsensusHub
    {
        //TODO: weigh necessity to make map PublicKey -> ConsensusGroupParticipant
        Dictionary<IKey, ConsensusGroupParticipant> GroupParticipants { get; }

        int TotalWeight { get; }
    }
}
