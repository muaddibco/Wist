using System.Collections.Generic;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.States;

namespace Wist.Node.Core.Common
{
    public interface INodeContext : IState
    {
        IKey AccountKey { get; }

        List<SynchronizationGroupParticipant> SyncGroupParticipants { get; }

        void Initialize(ISigningService signingService);

        ISigningService SigningService { get; }
    }
}
