using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Node.Core.Synchronization
{
    [ServiceContract]
    public interface ISyncRegistryMemPool
    {
        void AddCandidateBlock(RegistryFullBlock registryFullBlock);

        void AddVotingBlock(RegistryConfidenceBlock confidenceBlock);

        RegistryFullBlock GetMostConfidentFullBlock(ulong round);

        void ResetRound(ulong round);

        void SetLastCompletedSyncHeight(ulong syncHeight);
    }
}
