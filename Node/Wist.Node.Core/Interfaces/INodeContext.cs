using System.Collections.Generic;
using Wist.Core.States;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Interfaces
{
    public interface INodeContext : IState
    {
        ushort SyncGroupParticipantsCount { get; }

        /// <summary>
        /// Complete list of current participants involved into producing synchronization blocks
        /// </summary>
        List<ConsensusGroupParticipant> SyncGroupParticipants { get; }
    }
}
