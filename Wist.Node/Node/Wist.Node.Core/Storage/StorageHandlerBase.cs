using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Storage;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers.RawPackets;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.HashCalculations;
using Wist.Core.States;
using Wist.Network.Interfaces;
using Wist.Node.Core.Common;
using Wist.Node.Core.Registry;

namespace Wist.Node.Core.Storage
{
    public abstract class StorageHandlerBase<T> : IBlocksHandler where T : BlockBase
    {
        private readonly INodeContext _nodeContext;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IChainDataServicesManager _chainDataServicesManager;
        private ActionBlock<T> _storeBlock;
        private CancellationToken _cancellationToken;
        private readonly BlockingCollection<T> _registryBlocks;

        public StorageHandlerBase(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory, IRegistryMemPool registryMemPool, IConfigurationService configurationService, IHashCalculationsRepository hashCalculationRepository, IChainDataServicesManager chainDataServicesManager)
        {
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _chainDataServicesManager = chainDataServicesManager;
        }

        public abstract string Name { get; }

        public abstract PacketType PacketType { get; }

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _storeBlock = new ActionBlock<T>((Action<T>)StoreBlock, new ExecutionDataflowBlockOptions { BoundedCapacity = int.MaxValue,  CancellationToken = _cancellationToken, MaxDegreeOfParallelism = 1 });
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            _storeBlock.Post((T)blockBase);
        }

        private void StoreBlock(T blockBase)
        {
            IChainDataService chainDataService = _chainDataServicesManager.GetChainDataService(blockBase.PacketType);
            chainDataService.Add(blockBase);
        }
    }
}
