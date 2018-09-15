using System;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers;
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
using Wist.Node.Core.Modules;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SyncConfirmedSingleModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;

        public SyncConfirmedSingleModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ICryptoService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository, IHashCalculationsRepository hashCalculationRepository) 
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
            byte[] prevPowHash = _hashCalculation.CalculateHash(prevHash ?? new byte[Globals.DEFAULT_HASH_SIZE]);
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

                    ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(synchronizationConfirmedBlock);
                    signatureSupportSerializer.FillBodyAndRowBytes();

                    prevHash = CryptoHelper.ComputeHash(synchronizationConfirmedBlock.BodyBytes);
                    prevPowHash = _hashCalculation.CalculateHash(prevHash);

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
