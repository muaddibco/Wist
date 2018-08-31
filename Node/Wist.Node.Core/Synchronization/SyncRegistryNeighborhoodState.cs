using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.States;

namespace Wist.Node.Core.Synchronization
{
    public class SyncRegistryNeighborhoodState : NeighborhoodStateBase, ISyncRegistryNeighborhoodState
    {
        public static string NAME = "SyncRegistryNeighborhood";

        public override string Name => NAME;
    }
}
