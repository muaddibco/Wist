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
    public class SyncConfirmedSingleModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;

        public SyncConfirmedSingleModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ISigningService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository, IHashCalculationsRepository hashCalculationRepository) 
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository, hashCalculationRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
        }

        public override string Name => nameof(SyncConfirmedSingleModule);

        public override void Start()
        {
            ulong index = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;

            string cmd = null;
            byte[] prevHash = _synchronizationContext.LastBlockDescriptor?.Hash;
            byte[] prevPowHash = _proofOfWorkCalculation.CalculateHash(prevHash ?? new byte[Globals.DEFAULT_HASH_SIZE]);
            do
            {
                unchecked
                {
                    SynchronizationConfirmedBlock synchronizationConfirmedBlock = new SynchronizationConfirmedBlock()
                    {
                        SyncBlockHeight = index,
                        BlockHeight = ++index,
                        Signer = _key,
                        ReportedTime = DateTime.Now,
                        Round = 1,
                        HashPrev = prevHash ?? new byte[Globals.DEFAULT_HASH_SIZE],
                        PowHash = prevPowHash
                    };

                    ISerializer signatureSupportSerializer = _serializersFactory.Create(synchronizationConfirmedBlock);
                    signatureSupportSerializer.SerializeFully();

                    prevHash =  _hashCalculation.CalculateHash(synchronizationConfirmedBlock.RawData);
                    prevPowHash = _proofOfWorkCalculation.CalculateHash(prevHash);

                    _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);

                    _loadCountersService.SentMessages.Increment();
                }

                if(_cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine("Block sent. Press <Enter> for next or type 'exit' and press <Enter> for exit...");
                cmd = Console.ReadLine();
            } while (!_cancellationToken.IsCancellationRequested && cmd != "exit");
        }

        protected override void InitializeInner()
        {
            base.InitializeInner();

        }
    }
}
