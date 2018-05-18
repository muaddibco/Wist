using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Communication.Interfaces;
using Wist.Core.ExtensionMethods;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core
{
    // TODO: features
    // need to implement logic with time limit for confirmation of retransmitted blocks, etc
    // what happens when consensus was not achieved
    public class SynchronizationBlocksProcessor : IBlocksProcessor, IRequiresCommunicationHub
    {
        public const string BLOCKS_PROCESSOR_NAME = "SynchronizationBlocksProcessor";
        public const ushort TARGET_CONSENSUS_SIZE = 21;
        public const ushort TARGET_CONSENSUS_LOW_LIMIT = 14;

        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISynchronizationProducer _synchronizationProducer;
        private readonly INodeContext _nodeContext;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private ICommunicationHub _communicationHub;
        private uint _currentSyncBlockOrder;

        private readonly Dictionary<uint, Dictionary<string, List<SynchronizationBlockRetransmissionV1>>> _synchronizationBlocksByHeight;

        private readonly BlockingCollection<SynchronizationBlockBase> _synchronizationBlocks;
        private readonly BlockingCollection<SynchronizationBlockRetransmissionV1> _retransmittedBlocks;
        

        public SynchronizationBlocksProcessor(ISynchronizationContext synchronizationContext, ISynchronizationProducer synchronizationProducer, INodeContext nodeContext, ISignatureSupportSerializersFactory signatureSupportSerializersFactory)
        {
            _synchronizationContext = synchronizationContext;
            _synchronizationProducer = synchronizationProducer;
            _nodeContext = nodeContext;
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _synchronizationBlocks = new BlockingCollection<SynchronizationBlockBase>();
            _retransmittedBlocks = new BlockingCollection<SynchronizationBlockRetransmissionV1>();
            _synchronizationBlocksByHeight = new Dictionary<uint, Dictionary<string, List<SynchronizationBlockRetransmissionV1>>>();
        }

        public string Name => BLOCKS_PROCESSOR_NAME;

        public void Initialize(CancellationToken ct)
        {
            _currentSyncBlockOrder = _synchronizationContext.LastSyncBlock.BlockOrder;
            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);

            Task.Factory.StartNew(() =>
            {
                ProcessRetransmittedBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            SynchronizationBlockBase synchronizationBlock = blockBase as SynchronizationBlockBase;
            SynchronizationConfirmedBlock synchronizationConfirmedBlock = blockBase as SynchronizationConfirmedBlock;

            if (synchronizationBlock != null && !_synchronizationBlocks.IsAddingCompleted)
            {
                _synchronizationBlocks.TryAdd(synchronizationBlock);
            }
            else if (synchronizationConfirmedBlock != null && _synchronizationContext.LastSyncBlock.BlockOrder < synchronizationConfirmedBlock.BlockOrder)
            {
                _synchronizationContext.LastSyncBlock = synchronizationConfirmedBlock;
                _synchronizationContext.LastSyncBlockReceivingTime = DateTime.Now;
                _synchronizationProducer.DeferredBroadcast();
            }

            // if received ReadyForParticipationBlock and consensus on Synchronization not achieved yet so it is needed to involve joined participant into it. Otherwise it will be involved on the next loop.
            ReadyForParticipationBlock readyForParticipationBlock = blockBase as ReadyForParticipationBlock;
        }

        public void RegisterCommunicationHub(ICommunicationHub communicationHub)
        {
            _communicationHub = communicationHub;
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            List<SynchronizationBlockBase> synchronizationBlocksPerLoop = new List<SynchronizationBlockBase>();

            foreach (SynchronizationBlockBase synchronizationBlock in _synchronizationBlocks.GetConsumingEnumerable(ct))
            {
                if (_synchronizationContext.LastSyncBlock.BlockOrder + 1 > synchronizationBlock.BlockOrder)
                {
                    continue;
                }

                SynchronizationBlockV1 synchronizationBlockV1 = synchronizationBlock as SynchronizationBlockV1;

                if (synchronizationBlockV1 != null)
                {
                    if (_synchronizationContext.LastSyncBlock.BlockOrder + 1 == synchronizationBlockV1.BlockOrder)
                    {
                        RetransmitSynchronizationBlock(synchronizationBlockV1);
                    }
                    else
                    {
                        // TODO: need to understand what to do in case when confirmed synchronization block was not updated yet but next generation synchronization blocks already started to arrive
                    }
                }

                SynchronizationBlockRetransmissionV1 synchronizationBlockRetransmission = synchronizationBlock as SynchronizationBlockRetransmissionV1;
                if(synchronizationBlockRetransmission != null)
                {
                    _retransmittedBlocks.TryAdd(synchronizationBlockRetransmission);

                    if (_synchronizationContext.LastSyncBlock.BlockOrder + 1 == synchronizationBlockRetransmission.BlockOrder)
                    {

                    }
                    else
                    {
                        // TODO: need to understand what to do in case when confirmed synchronization block was not updated yet but next generation synchronization retransmission blocks already started to arrive
                    }
                }
            }
        }

        private void ProcessRetransmittedBlocks(CancellationToken ct)
        {
            foreach (SynchronizationBlockRetransmissionV1 retransmittedBlock in _retransmittedBlocks.GetConsumingEnumerable(ct))
            {
                if(!_synchronizationBlocksByHeight.ContainsKey(retransmittedBlock.BlockOrder))
                {
                    _synchronizationBlocksByHeight.Add(retransmittedBlock.BlockOrder, new Dictionary<string, List<SynchronizationBlockRetransmissionV1>>());
                }

                string publicKey = retransmittedBlock.ConfirmationPublicKey.ToHexString();
                if(!_synchronizationBlocksByHeight[retransmittedBlock.BlockOrder].ContainsKey(publicKey))
                {
                    _synchronizationBlocksByHeight[retransmittedBlock.BlockOrder].Add(publicKey, new List<SynchronizationBlockRetransmissionV1>());
                }

                if(!_synchronizationBlocksByHeight[retransmittedBlock.BlockOrder][publicKey].Any(s => s.PublicKey.Equals32(retransmittedBlock.PublicKey)))
                {
                    _synchronizationBlocksByHeight[retransmittedBlock.BlockOrder][publicKey].Add(retransmittedBlock);
                }

                if(CheckSynchronizationCompleteConsensusAchieved(retransmittedBlock.BlockOrder))
                {
                    BroadcastConfirmation(retransmittedBlock.BlockOrder);
                }
            }
        }

        private void BroadcastConfirmation(uint height)
        {
            List<SynchronizationBlockRetransmissionV1> retransmittedSyncBlocks = _synchronizationBlocksByHeight[height].Where(r => _synchronizationContext.Participants.Any(p => p.PublicKeyString == r.Key)).Select(kv => kv.Value.First()).OrderBy(s => s.ConfirmationPublicKey.ToHexString()).ToList();

            SynchronizationConfirmedBlock synchronizationConfirmedBlock = new SynchronizationConfirmedBlock
            {
                BlockOrder = height,
                ReportedTimes = retransmittedSyncBlocks.Select(b => b.ReportedTime).ToArray(),
                PublicKeys = retransmittedSyncBlocks.Select(b => b.ConfirmationPublicKey).ToArray(),
                Signatures = retransmittedSyncBlocks.Select(b => b.ConfirmationSignature).ToArray()
            };

            _communicationHub.BroadcastMessage(synchronizationConfirmedBlock);
        }

        private bool CheckSynchronizationCompleteConsensusAchieved(uint height)
        {
            if (!_synchronizationBlocksByHeight.ContainsKey(height))
            {
                return false;
            }

            ushort count = 0;
            Dictionary<string, List<SynchronizationBlockRetransmissionV1>> retransmittedSyncBlocks = _synchronizationBlocksByHeight[height];
            foreach (string publicKey in retransmittedSyncBlocks.Keys)
            {
                if (_synchronizationContext.Participants.Any(p => p.PublicKeyString == publicKey))
                {
                    IEnumerable<SynchronizationBlockRetransmissionV1> lst = retransmittedSyncBlocks[publicKey].Where(s => _synchronizationContext.Participants.Any(p => p.PublicKey.Equals32(s.ConfirmationPublicKey)));
                    if (lst.Count() >= TARGET_CONSENSUS_LOW_LIMIT && lst.Any(l => lst.First().ReportedTime == l.ReportedTime))
                    {
                        count++;
                    }
                }
            }

            return count == TARGET_CONSENSUS_SIZE;
        }

        private async void RetransmitSynchronizationBlock(SynchronizationBlockV1 synchronizationBlockV1)
        {
            SynchronizationBlockRetransmissionV1 synchronizationBlockRetransmissionForSend = new SynchronizationBlockRetransmissionV1()
            {
                BlockOrder = synchronizationBlockV1.BlockOrder,
                ReportedTime = synchronizationBlockV1.ReportedTime,
                OffsetSinceLastMedian = (ushort)(DateTime.Now - _synchronizationContext.LastSyncBlockReceivingTime).TotalSeconds,
                ConfirmationPublicKey = synchronizationBlockV1.PublicKey,
                ConfirmationSignature = synchronizationBlockV1.Signature,
                PublicKey = _nodeContext.PublicKey
            };

            byte[] body = _signatureSupportSerializersFactory.Create(ChainType.Synchronization, BlockTypes.Synchronization_RetransmissionBlock).GetBody(synchronizationBlockRetransmissionForSend);
            synchronizationBlockRetransmissionForSend.Signature = _nodeContext.Sign(body);

            await _communicationHub.BroadcastMessage(synchronizationBlockRetransmissionForSend);

            _retransmittedBlocks.TryAdd(synchronizationBlockRetransmissionForSend);
        }

        #endregion Private Functions
    }
}
