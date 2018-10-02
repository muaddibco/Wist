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
        IDisposable SubscribeOnRoundElapsed(ITargetBlock<RoundDescriptor> onRoundElapsed);

        void AddCandidateBlock(RegistryFullBlock registryFullBlock);

        void AddVotingBlock(RegistryConfidenceBlock confidenceBlock);

        RegistryFullBlock GetMostConfidentFullBlock(RoundDescriptor roundDescriptor);
    }
}
