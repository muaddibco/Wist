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
using Wist.Core.Architecture;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Core.Identity;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core
{
    //TODO: features
    // need to implement logic with time limit for confirmation of retransmitted blocks, etc
    // what happens when consensus was not achieved
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class SynchronizationBlocksProcessor : IBlocksHandler, IRequiresCommunicationHub
    {
        public const string BLOCKS_PROCESSOR_NAME = "SynchronizationBlocksProcessor";
        public const ushort TARGET_CONSENSUS_SIZE = 21;
        public const ushort TARGET_CONSENSUS_LOW_LIMIT = 14;

        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISynchronizationProducer _synchronizationProducer;
        private readonly INodeContext _nodeContext;
        private readonly IAccountState _accountState;
        private readonly ISignatureSupportSerializersFactory _signatureSupportSerializersFactory;
        private readonly ICryptoService _cryptoService;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private IServerCommunicationService _communicationHub;
        private ulong _currentSyncBlockOrder;

        private readonly Dictionary<ulong, Dictionary<IKey, List<SynchronizationBlockRetransmissionV1>>> _synchronizationBlocksByHeight;

        private readonly BlockingCollection<SynchronizationBlockBase> _synchronizationBlocks;
        private readonly BlockingCollection<SynchronizationBlockRetransmissionV1> _retransmittedBlocks;
        

        public SynchronizationBlocksProcessor(IStatesRepository statesRepository, ISynchronizationProducer synchronizationProducer, ISignatureSupportSerializersFactory signatureSupportSerializersFactory, ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)
        {
            _synchronizationContext = statesRepository.GetInstance<Wist.Core.Synchronization.SynchronizationContext>();
            _synchronizationProducer = synchronizationProducer;
            _nodeContext = statesRepository.GetInstance<NodeContext>();
            _accountState = statesRepository.GetInstance<AccountState>();
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _cryptoService = cryptoService;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _synchronizationBlocks = new BlockingCollection<SynchronizationBlockBase>();
            _retransmittedBlocks = new BlockingCollection<SynchronizationBlockRetransmissionV1>();
            _synchronizationBlocksByHeight = new Dictionary<ulong, Dictionary<IKey, List<SynchronizationBlockRetransmissionV1>>>();
        }

        public string Name => BLOCKS_PROCESSOR_NAME;

        public PacketType PacketType => PacketType.Synchronization;

        public void Initialize(CancellationToken ct)
        {
            _currentSyncBlockOrder = _synchronizationContext.LastBlockDescriptor.BlockHeight;
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
            else if (synchronizationConfirmedBlock != null && _synchronizationContext.LastBlockDescriptor.BlockHeight < synchronizationConfirmedBlock.BlockHeight)
            {
                _synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(synchronizationConfirmedBlock.BlockHeight, synchronizationConfirmedBlock.HashPrev, synchronizationConfirmedBlock.ReportedTime, DateTime.Now));
                _synchronizationProducer.DeferredBroadcast();
            }

            // if received ReadyForParticipationBlock and consensus on Synchronization not achieved yet so it is needed to involve joined participant into it. Otherwise it will be involved on the next loop.
            ReadyForParticipationBlock readyForParticipationBlock = blockBase as ReadyForParticipationBlock;
        }

        public void RegisterCommunicationHub(IServerCommunicationService communicationHub)
        {
            _communicationHub = communicationHub;
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            List<SynchronizationBlockBase> synchronizationBlocksPerLoop = new List<SynchronizationBlockBase>();

            foreach (SynchronizationBlockBase synchronizationBlock in _synchronizationBlocks.GetConsumingEnumerable(ct))
            {
                if (_synchronizationContext.LastBlockDescriptor.BlockHeight + 1 > synchronizationBlock.BlockHeight)
                {
                    continue;
                }

                SynchronizationProducingBlock synchronizationBlockV1 = synchronizationBlock as SynchronizationProducingBlock;

                if (synchronizationBlockV1 != null)
                {
                    if (_synchronizationContext.LastBlockDescriptor.BlockHeight + 1 == synchronizationBlockV1.BlockHeight)
                    {
                        RetransmitSynchronizationBlock(synchronizationBlockV1);
                    }
                    else
                    {
                        //TODO: need to understand what to do in case when confirmed synchronization block was not updated yet but next generation synchronization blocks already started to arrive
                    }
                }

                SynchronizationBlockRetransmissionV1 synchronizationBlockRetransmission = synchronizationBlock as SynchronizationBlockRetransmissionV1;
                if(synchronizationBlockRetransmission != null)
                {
                    _retransmittedBlocks.TryAdd(synchronizationBlockRetransmission);

                    if (_synchronizationContext.LastBlockDescriptor.BlockHeight + 1 == synchronizationBlockRetransmission.BlockHeight)
                    {

                    }
                    else
                    {
                        //TODO: need to understand what to do in case when confirmed synchronization block was not updated yet but next generation synchronization retransmission blocks already started to arrive
                    }
                }
            }
        }

        private void ProcessRetransmittedBlocks(CancellationToken ct)
        {
            foreach (SynchronizationBlockRetransmissionV1 retransmittedBlock in _retransmittedBlocks.GetConsumingEnumerable(ct))
            {
                if(!_synchronizationBlocksByHeight.ContainsKey(retransmittedBlock.BlockHeight))
                {
                    _synchronizationBlocksByHeight.Add(retransmittedBlock.BlockHeight, new Dictionary<IKey, List<SynchronizationBlockRetransmissionV1>>());
                }

                IKey publicKey = _identityKeyProvider.GetKey(retransmittedBlock.ConfirmationPublicKey);
                if(!_synchronizationBlocksByHeight[retransmittedBlock.BlockHeight].ContainsKey(publicKey))
                {
                    _synchronizationBlocksByHeight[retransmittedBlock.BlockHeight].Add(publicKey, new List<SynchronizationBlockRetransmissionV1>());
                }

                if(!_synchronizationBlocksByHeight[retransmittedBlock.BlockHeight][publicKey].Any(s => s.Key.Equals(retransmittedBlock.Key)))
                {
                    _synchronizationBlocksByHeight[retransmittedBlock.BlockHeight][publicKey].Add(retransmittedBlock);
                }

                if(CheckSynchronizationCompleteConsensusAchieved(retransmittedBlock.BlockHeight))
                {
                    BroadcastConfirmation(retransmittedBlock.BlockHeight);
                }
            }
        }

        private void BroadcastConfirmation(ulong height)
        {
            List<SynchronizationBlockRetransmissionV1> retransmittedSyncBlocks = _synchronizationBlocksByHeight[height].Where(r => _nodeContext.SyncGroupParticipants.Any(p => p.PublicKey == r.Key)).Select(kv => kv.Value.First()).OrderBy(s => s.ConfirmationPublicKey.ToHexString()).ToList();

            SynchronizationConfirmedBlock synchronizationConfirmedBlock = new SynchronizationConfirmedBlock
            {
                BlockHeight = height,
                //ReportedTimes = retransmittedSyncBlocks.Select(b => b.ReportedTime).ToArray(),
                PublicKeys = retransmittedSyncBlocks.Select(b => b.ConfirmationPublicKey).ToArray(),
                Signatures = retransmittedSyncBlocks.Select(b => b.ConfirmationSignature).ToArray()
            };

            //TODO: complete logic of sync block propagation
            //_communicationHub.PostMessage(synchronizationConfirmedBlock);
        }

        private bool CheckSynchronizationCompleteConsensusAchieved(ulong height)
        {
            if (!_synchronizationBlocksByHeight.ContainsKey(height))
            {
                return false;
            }

            ushort count = 0;
            Dictionary<IKey, List<SynchronizationBlockRetransmissionV1>> retransmittedSyncBlocks = _synchronizationBlocksByHeight[height];
            foreach (IKey publicKey in retransmittedSyncBlocks.Keys)
            {
                if (_nodeContext.SyncGroupParticipants.Any(p => p.PublicKey == publicKey))
                {
                    IEnumerable<SynchronizationBlockRetransmissionV1> lst = retransmittedSyncBlocks[publicKey].Where(s => _nodeContext.SyncGroupParticipants.Any(p => p.PublicKey.Equals(s.ConfirmationPublicKey)));
                    if (lst.Count() >= TARGET_CONSENSUS_LOW_LIMIT && lst.Any(l => lst.First().ReportedTime == l.ReportedTime))
                    {
                        count++;
                    }
                }
            }

            return count == TARGET_CONSENSUS_SIZE;
        }

        private async void RetransmitSynchronizationBlock(SynchronizationProducingBlock synchronizationBlockV1)
        {
            SynchronizationBlockRetransmissionV1 synchronizationBlockRetransmissionForSend = new SynchronizationBlockRetransmissionV1()
            {
                BlockHeight = synchronizationBlockV1.BlockHeight,
                ReportedTime = synchronizationBlockV1.ReportedTime,
                OffsetSinceLastMedian = (ushort)(DateTime.Now - _synchronizationContext.LastBlockDescriptor.UpdateTime).TotalSeconds,
                ConfirmationPublicKey = synchronizationBlockV1.Key.Value,
                ConfirmationSignature = synchronizationBlockV1.Signature,
                Key = _accountState.AccountKey
            };

            //TODO: refactor
            //byte[] body = _signatureSupportSerializersFactory.Create(PacketType.Synchronization, BlockTypes.Synchronization_RetransmissionBlock).GetBytes(synchronizationBlockRetransmissionForSend);
            //synchronizationBlockRetransmissionForSend.Signature = _cryptoService.Sign(body);

            //TODO: complete logic of sync block propagation
            //await _communicationHub.PostMessage(synchronizationBlockRetransmissionForSend);

            _retransmittedBlocks.TryAdd(synchronizationBlockRetransmissionForSend);
        }

        #endregion Private Functions
    }
}
