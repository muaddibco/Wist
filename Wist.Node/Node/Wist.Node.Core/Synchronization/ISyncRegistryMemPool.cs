using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Wist.Blockchain.Core.DataModel.Registry;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Core.Architecture;
using Wist.Core.Identity;

namespace Wist.Node.Core.Synchronization
{
    [ServiceContract]
    public interface ISyncRegistryMemPool
    {
        void AddCandidateBlock(RegistryFullBlock registryFullBlock);

        void RegisterCombinedBlock(SynchronizationRegistryCombinedBlock combinedBlock);

        IEnumerable<RegistryFullBlock> GetRegistryBlocks();
    }
}
