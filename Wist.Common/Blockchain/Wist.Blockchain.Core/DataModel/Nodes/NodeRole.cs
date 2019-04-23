using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Blockchain.Core.DataModel.Nodes
{
    public enum NodeRole : byte
    {
        TransactionsRegistrationLayer,
        StorageLayer,
        DeferredConsensusLayer,
        SynchronizationLayer
    }
}
