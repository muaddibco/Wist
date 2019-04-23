using System;
using System.Linq;
using Wist.Blockchain.Core;
using Wist.Blockchain.Core.DataModel.Synchronization;
using Wist.Blockchain.Core.Enums;
using Wist.Blockchain.Core.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.HashCalculations;
using Wist.Core.Logging;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.Node.Core.Synchronization
{
    /// <summary>
    /// Set SynchronizationContext according to information in database
    /// </summary>
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationInitializer : InitializerBase
    {
        private readonly ILogger _logger;
        private readonly IChainDataService _chainDataService;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IHashCalculation _hashCalculation;

        public SynchronizationInitializer(IStatesRepository statesRepository, IChainDataServicesManager chainDataServicesManager, ILoggerService loggerService, IHashCalculationsRepository hashCalculationsRepository)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _chainDataService = chainDataServicesManager.GetChainDataService(PacketType.Synchronization);
            _logger = loggerService.GetLogger(typeof(SynchronizationInitializer).Name);
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override void InitializeInner()
        {
            _logger.Info("Starting Synchronization Initializer");

            try
            {
                SynchronizationConfirmedBlock synchronizationConfirmedBlock = (SynchronizationConfirmedBlock)_chainDataService.GetAllLastBlocksByType(BlockTypes.Synchronization_ConfirmedBlock).Single();

                if (synchronizationConfirmedBlock != null)
                {
                    _synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(synchronizationConfirmedBlock.BlockHeight, _hashCalculation.CalculateHash(synchronizationConfirmedBlock.RawData), synchronizationConfirmedBlock.ReportedTime, DateTime.Now, synchronizationConfirmedBlock.Round));
                }

                SynchronizationRegistryCombinedBlock combinedBlock = (SynchronizationRegistryCombinedBlock)_chainDataService.GetAllLastBlocksByType(BlockTypes.Synchronization_RegistryCombinationBlock).Single();
                if(combinedBlock != null)
                {
                    _synchronizationContext.LastRegistrationCombinedBlockHeight = combinedBlock.BlockHeight;
                }
            }
            finally
            {
                _logger.Info("Synchronization Initializer completed");
            }
        }
    }
}
