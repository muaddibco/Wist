using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.Core.Architecture;

namespace Wist.Core.Shards
{
    [ServiceContract]
    public interface IShardsManager
    {
        void Initialize(CancellationToken cancellationToken);
        IEnumerable<ShardDescriptor> GetAllRegistryShards();

        void UpdateTransactionalShards();
    }
}
