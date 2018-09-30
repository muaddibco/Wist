using System;
using System.Linq;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Cryptography;
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

        public SynchronizationInitializer(IStatesRepository statesRepository, IChainDataServicesManager chainDataServicesManager, ILoggerService loggerService)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _chainDataService = chainDataServicesManager.GetChainDataService(PacketType.Synchronization);
            _logger = loggerService.GetLogger(typeof(SynchronizationInitializer).Name);
        }

        protected override void InitializeInner()
        {
            _logger.Info("Starting Synchronization Initializer");

            try
            {
                SynchronizationConfirmedBlock synchronizationConfirmedBlock = (SynchronizationConfirmedBlock)_chainDataService.GetAllLastBlocksByType(BlockTypes.Synchronization_ConfirmedBlock).Single();

                if (synchronizationConfirmedBlock != null)
                {
                    _synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(synchronizationConfirmedBlock.BlockHeight, CryptoHelper.ComputeHash(synchronizationConfirmedBlock.BodyBytes), synchronizationConfirmedBlock.ReportedTime, DateTime.Now, synchronizationConfirmedBlock.Round));
                }
            }
            finally
            {
                _logger.Info("Synchronization Initializer completed");
            }
        }
    }
}
