using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Models;
using Wist.Core.States;

namespace Wist.Node.Core.Synchronization
{
    public interface ISynchronizationGroupState : IState
    {
        IEnumerable<IKey> GetAllParticipants();

        bool CheckParticipant(IKey key);

        bool AddParticipant(IKey key);

        bool RemoveParticipant(IKey key);
    }
}
