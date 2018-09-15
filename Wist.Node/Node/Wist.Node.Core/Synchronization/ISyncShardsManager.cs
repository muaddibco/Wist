using System;
using System.Collections.Generic;
using System.Text;
using Wist.Core.Architecture;
using Wist.Core.Shards;

namespace Wist.Node.Core.Synchronization
{
    [ServiceContract]
    public interface ISyncShardsManager
    {
        ShardDescriptor GetShardDescriptorByRound(int round);
    }
}
