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
using Wist.Node.Core.Registry;

namespace Wist.Node.Core.Storage
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class StorageHandler : IBlocksHandler
    {
        public const string NAME = "Storage";

        private readonly INodeContext _nodeContext;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;

        private ActionBlock<StorageTransactionFullBlock> _storeFullBlock;
        private CancellationToken _cancellationToken;

        public StorageHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory, IRegistryMemPool registryMemPool, IConfigurationService configurationService, IHashCalculationsRepository hashCalculationRepository)
        {
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _communicationServicesRegistry = communicationServicesRegistry;
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Storage;

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _storeFullBlock = new ActionBlock<StorageTransactionFullBlock>((Action<StorageTransactionFullBlock>)StoreFullBlock, new ExecutionDataflowBlockOptions { CancellationToken = _cancellationToken });
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            StorageBlockBase storageBlockBase = blockBase as StorageBlockBase;

            if (storageBlockBase == null)
            {
                return;
            }

            StorageTransactionFullBlock storageTransactionFullBlock = blockBase as StorageTransactionFullBlock;

            _storeFullBlock.Post(storageTransactionFullBlock);
        }

        private void StoreFullBlock(StorageTransactionFullBlock storageTransactionFullBlock)
        {

        }
    }
}
