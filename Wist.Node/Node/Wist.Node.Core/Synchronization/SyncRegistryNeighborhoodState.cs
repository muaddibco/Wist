using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class SyncRegistryNeighborhoodState : NeighborhoodStateBase, ISyncRegistryNeighborhoodState
    {
        public override string Name => nameof(ISyncRegistryNeighborhoodState);
    }
}
