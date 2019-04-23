﻿using Wist.Blockchain.Core.DataModel.Transactional;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.Core.Serializers.RawPackets;
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
    public class TransactionalStorageHandler : StorageHandlerBase<TransactionalPacketBase>
    {
        public const string NAME = "TransactionalStorage";
        public TransactionalStorageHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IRawPacketProvidersFactory rawPacketProvidersFactory, IRegistryMemPool registryMemPool, IConfigurationService configurationService, IHashCalculationsRepository hashCalculationRepository, IChainDataServicesManager chainDataServicesManager) 
            : base(statesRepository, communicationServicesRegistry, rawPacketProvidersFactory, registryMemPool, configurationService, hashCalculationRepository, chainDataServicesManager)
        {
        }

        public override string Name => NAME;

        public override PacketType PacketType => PacketType.Transactional;
    }
}
