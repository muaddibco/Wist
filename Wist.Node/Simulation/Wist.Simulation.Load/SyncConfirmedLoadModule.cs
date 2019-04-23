using System;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Interfaces;
using Wist.Blockchain.Core.Serializers;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.HashCalculations;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Core.Modularity;
using Wist.Core;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SyncConfirmedLoadModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;

        public SyncConfirmedLoadModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ISigningService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository, IHashCalculationsRepository hashCalculationRepository) 
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository, hashCalculationRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
        }

        public override string Name => nameof(SyncConfirmedLoadModule);

        public override void Start()
        {

            ulong index = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;

            byte[] prevHash = _synchronizationContext.LastBlockDescriptor?.Hash;

            do
            {
                unchecked
                {
                    SynchronizationConfirmedBlock synchronizationConfirmedBlock = new SynchronizationConfirmedBlock()
                    {
                        SyncBlockHeight = index,
                        BlockHeight = (ulong)++index,
                        Signer = _key,
                        ReportedTime = DateTime.Now,
                        Round = 1,
                        HashPrev = prevHash ?? new byte[Globals.DEFAULT_HASH_SIZE],
                        PowHash = prevHash ?? new byte[Globals.DEFAULT_HASH_SIZE]
                    };

                    ISerializer signatureSupportSerializer = _serializersFactory.Create(synchronizationConfirmedBlock);
                    signatureSupportSerializer.SerializeFully();

                    prevHash = _hashCalculation.CalculateHash(synchronizationConfirmedBlock.RawData);

                    _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);

                    _loadCountersService.SentMessages.Increment();
                }
            } while (!_cancellationToken.IsCancellationRequested);
        }

        protected override void InitializeInner()
        {
            base.InitializeInner();
        }
    }
}
