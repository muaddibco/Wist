using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.DataModel.Nodes;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Shards;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.Network.Topology
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

            UpdateTransactionalShards();
        }

        public void UpdateTransactionalShards()
        {
            lock(_sync)
            {
                IEnumerable<Node> nodes = _nodesDataService.GetAll().Where(n => n.NodeRole == NodeRole.TransactionsRegistrationLayer);

                List<ShardDescriptor> shardDescriptors = new List<ShardDescriptor>();
                ShardDescriptor shardDescriptor = new ShardDescriptor();
                shardDescriptors.Add(shardDescriptor);

                foreach (var node in nodes)
                {
                    //TODO: apply usage of _synchronizationContext.LastBlockDescriptor for building shards
                    shardDescriptor.Nodes.Add(node.Key);
                }

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
