﻿using System;
using Wist.BlockLattice.Core;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Cryptography;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.Core.PerformanceCounters;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;

namespace Wist.Simulation.Load
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class SyncConfirmedSingleModule : LoadModuleBase
    {
        private readonly ISynchronizationContext _synchronizationContext;

        public SyncConfirmedSingleModule(ILoggerService loggerService, IClientCommunicationServiceRepository clientCommunicationServiceRepository, IConfigurationService configurationService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, INodesDataService nodesDataService, ICryptoService cryptoService, IPerformanceCountersRepository performanceCountersRepository, IStatesRepository statesRepository) 
            : base(loggerService, clientCommunicationServiceRepository, configurationService, identityKeyProvidersRegistry, signatureSupportSerializersFactory, nodesDataService, cryptoService, performanceCountersRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<SynchronizationContext>();
        }

        public override string Name => nameof(SyncConfirmedSingleModule);

        protected override void InitializeInner()
        {
            base.InitializeInner();

            ulong index = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;

            string cmd = null;
            byte[] prevHash = _synchronizationContext.LastBlockDescriptor?.Hash;
            do
            {
                unchecked
                {
                    SynchronizationConfirmedBlock synchronizationConfirmedBlock = new SynchronizationConfirmedBlock()
                    {
                        SyncBlockHeight = index,
                        BlockHeight = ++index,
                        Key = _key,
                        ReportedTime = DateTime.Now,
                        Round = 1,
                        HashPrev = prevHash ?? new byte[Globals.HASH_SIZE]
                    };

                    ISignatureSupportSerializer signatureSupportSerializer = _signatureSupportSerializersFactory.Create(synchronizationConfirmedBlock);
                    signatureSupportSerializer.FillBodyAndRowBytes();

                    prevHash = CryptoHelper.ComputeHash(synchronizationConfirmedBlock.BodyBytes);

                    _communicationService.PostMessage(_keyTarget, signatureSupportSerializer);

                    _loadCountersService.SentMessages.Increment();
                }

                Console.WriteLine("Block sent. Press <Enter> for next or type 'exit' and press <Enter> for exit...");
                cmd = Console.ReadLine();
            } while (cmd != "exit");
        }
    }
}