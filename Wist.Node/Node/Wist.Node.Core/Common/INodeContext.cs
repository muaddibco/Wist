using System.Collections.Generic;
using Wist.Core.Identity;
using Wist.Core.States;

namespace Wist.Node.Core.Common
{
    public interface INodeContext : IState
    {
        IKey NodeKey { get; }

        List<SynchronizationGroupParticipant> SyncGroupParticipants { get; }

        void Initialize();
    }
}
