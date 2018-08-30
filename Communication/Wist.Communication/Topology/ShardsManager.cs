using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Shards;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.Communication.Topology
{
    [RegisterDefaultImplementation(typeof(IShardsManager), Lifetime = LifetimeManagement.Singleton)]
    public class ShardsManager : IShardsManager
    {
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IDisposable _syncContextChangedUnsibscriber;
        private readonly object _sync = new object();
        private readonly INodesDataService _nodesDataService;
        private IEnumerable<ShardDescriptor> _shardDescriptors;
        private CancellationToken _cancellationToken;

        public ShardsManager(IStatesRepository statesRepository, INodesDataService nodesDataService)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _syncContextChangedUnsibscriber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)OnSyncContextChanged));
            _nodesDataService = nodesDataService;
        }

        public IEnumerable<ShardDescriptor> GetAllRegistryShards()
        {
            return _shardDescriptors;
        }

        public void Initialize(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _cancellationToken.Register(() => _syncContextChangedUnsibscriber.Dispose());

            IEnumerable<BlockLattice.Core.DataModel.Nodes.Node> nodes = _nodesDataService.GetAll();

            List<ShardDescriptor> shardDescriptors = new List<ShardDescriptor>();
            ShardDescriptor shardDescriptor = new ShardDescriptor();
            shardDescriptors.Add(shardDescriptor);

            foreach (var node in nodes)
            {
                //TODO: apply usage of _synchronizationContext.LastBlockDescriptor for building shards
                shardDescriptor.Nodes.Add(node.Key);
            }
        }

        public void UpdateTransactionalShards()
        {
            lock(_sync)
            {
                List<ShardDescriptor> shardDescriptors = new List<ShardDescriptor>();
                _shardDescriptors = new ReadOnlyCollection<ShardDescriptor>(shardDescriptors);
            }
        }

        #region Private Functions

        private void OnSyncContextChanged(string propName)
        {
            UpdateTransactionalShards();
        }

        #endregion Private Functions
    }
}
