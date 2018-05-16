using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core
{
    public class SynchronizationBlocksProcessor : IBlocksProcessor, IRequiresCommunicationHub
    {
        public const string BLOCKS_PROCESSOR_NAME = "SynchronizationBlocksProcessor";

        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISynchronizationProducer _synchronizationProducer;
        private ICommunicationHub _communicationHub;

        public SynchronizationBlocksProcessor(ISynchronizationContext synchronizationContext, ISynchronizationProducer synchronizationProducer)
        {
            _synchronizationContext = synchronizationContext;
            _synchronizationProducer = synchronizationProducer;
        }

        public string Name => BLOCKS_PROCESSOR_NAME;

        public void Initialize(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            SynchronizationBlockBase synchronizationBlock = blockBase as SynchronizationBlockBase;
            SynchronizationConfirmedBlock synchronizationConfirmedBlock = blockBase as SynchronizationConfirmedBlock;

            if (synchronizationBlock != null)
            {
            }

            if (synchronizationConfirmedBlock != null && _synchronizationContext.LastSyncBlock.BlockOrder < synchronizationConfirmedBlock.BlockOrder)
            {
                _synchronizationContext.LastSyncBlock = synchronizationConfirmedBlock;
                _synchronizationContext.LastSyncBlockReceivingTime = DateTime.Now;
                _synchronizationProducer.Launch();
            }

            // if received ReadyForParticipationBlock and consensus on Synchronization not achieved yet so it is needed to involve joined participant into it. Otherwise it will be involved on the next loop.
            ReadyForParticipationBlock readyForParticipationBlock = blockBase as ReadyForParticipationBlock;
        }

        public void RegisterCommunicationHub(ICommunicationHub communicationHub)
        {
            _communicationHub = communicationHub;
        }
    }
}
