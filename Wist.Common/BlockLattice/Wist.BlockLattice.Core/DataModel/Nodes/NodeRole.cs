using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.BlockLattice.Core.DataModel.Nodes
{
    public enum NodeRole : byte
    {
        TransactionsRegistrationLayer,
        StorageLayer,
        DeferredConsensusLayer,
        SynchronizationLayer
    }
}
