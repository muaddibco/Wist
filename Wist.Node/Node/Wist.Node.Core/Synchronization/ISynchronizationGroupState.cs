using System.Collections.Generic;
using Wist.Core.Communication;
using Wist.Core.Identity;
using Wist.Core.States;

namespace Wist.Node.Core.Synchronization
{
    public interface ISynchronizationGroupState : INeighborhoodState
    {
        bool CheckParticipant(IKey key);
    }
}
