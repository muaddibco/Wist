using System.Linq;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Shards;

namespace Wist.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISyncShardsManager), Lifetime = LifetimeManagement.Singleton)]
    public class SyncShardsManager : ISyncShardsManager
    {
        private readonly IShardsManager _shardsManager;

        public SyncShardsManager(IShardsManager shardsManager)
        {
            _shardsManager = shardsManager;
        }

        public ShardDescriptor GetShardDescriptorByRound(int round)
        {
            return _shardsManager.GetAllRegistryShards()?.First();
        }
    }
}
