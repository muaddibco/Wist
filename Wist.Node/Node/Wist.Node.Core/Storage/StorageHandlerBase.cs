using System;
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
    public abstract class StorageHandlerBase : IBlocksHandler
    {
        private readonly INodeContext _nodeContext;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IChainDataServicesManager _chainDataServicesManager;
        private ActionBlock<BlockBase> _storeFullBlock;
        private CancellationToken _cancellationToken;

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
            _storeFullBlock = new ActionBlock<BlockBase>((Action<BlockBase>)StoreFullBlock, new ExecutionDataflowBlockOptions { CancellationToken = _cancellationToken });
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            _storeFullBlock.Post(blockBase);
        }

        private void StoreFullBlock(BlockBase blockBase)
        {
            IChainDataService chainDataService = _chainDataServicesManager.GetChainDataService(blockBase.PacketType);
            chainDataService.Add(blockBase);
        }
    }
}
