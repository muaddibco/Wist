using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Synchronization;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.BlockLattice.Core.Serializers;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Cryptography;
using Wist.Core.ExtensionMethods;
using Wist.Core.Identity;
using Wist.Core.States;
using Wist.Core.Synchronization;
using Wist.Node.Core.Common;
using Wist.Node.Core.Rating;
using Wist.Network.Topology;
using Wist.Core.Communication;
using Wist.BlockLattice.Core;

namespace Wist.Node.Core.Synchronization
{
    //TODO: features
    // need to implement logic with time limit for confirmation of retransmitted blocks, etc
    // what happens when consensus was not achieved
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = Wist.Core.Architecture.Enums.LifetimeManagement.Singleton)]
    public class SynchronizationBlocksHandler : IBlocksHandler
    {
        public const string NAME = "SynchronizationBlocksHandler";
        public const ushort TARGET_CONSENSUS_SIZE = 21;
        public const ushort TARGET_CONSENSUS_LOW_LIMIT = 1;// 14;

        private readonly ISynchronizationContext _synchronizationContext;
        private readonly ISynchronizationProducer _synchronizationProducer;
        private readonly INodeContext _nodeContext;
        private readonly IAccountState _accountState;
        private readonly ISerializersFactory _signatureSupportSerializersFactory;
        private readonly ICryptoService _cryptoService;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly INodesRatingProvider _nodesRatingProvider;
        private readonly INeighborhoodState _neighborhoodState;
        private IServerCommunicationService _communicationService;
        private ulong _currentSyncBlockOrder;

        private readonly Dictionary<ulong, Dictionary<IKey, List<SynchronizationBlockRetransmissionV1>>> _synchronizationBlocksByHeight;

        private readonly BlockingCollection<SynchronizationBlockBase> _synchronizationBlocks;
        private readonly BlockingCollection<SynchronizationBlockRetransmissionV1> _retransmittedBlocks;
        

        public SynchronizationBlocksHandler(IStatesRepository statesRepository, ISynchronizationProducer synchronizationProducer, ISerializersFactory signatureSupportSerializersFactory, 
            ICryptoService cryptoService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, INodesRatingProviderFactory nodesRatingProvidersFactory, IServerCommunicationServicesRegistry communicationServicesRegistry)
        {
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _synchronizationProducer = synchronizationProducer;
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _accountState = statesRepository.GetInstance<IAccountState>();
            _neighborhoodState = statesRepository.GetInstance<INeighborhoodState>();
            _signatureSupportSerializersFactory = signatureSupportSerializersFactory;
            _cryptoService = cryptoService;
            _communicationServicesRegistry = communicationServicesRegistry;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _synchronizationBlocks = new BlockingCollection<SynchronizationBlockBase>();
            _retransmittedBlocks = new BlockingCollection<SynchronizationBlockRetransmissionV1>();
            _synchronizationBlocksByHeight = new Dictionary<ulong, Dictionary<IKey, List<SynchronizationBlockRetransmissionV1>>>();
            _nodesRatingProvider = nodesRatingProvidersFactory.GetInstance(PacketType.Transactional);
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Synchronization;

        public void Initialize(CancellationToken ct)
        {
            _currentSyncBlockOrder = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;
            _communicationService = _communicationServicesRegistry.GetInstance("GenericTcp");

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

            if (synchronizationBlock != null && !_synchronizationBlocks.IsAddingCompleted)
            {
                _synchronizationBlocks.TryAdd(synchronizationBlock);
            }
        }

        #region Private Functions

        private void ProcessBlocks(CancellationToken ct)
        {
            List<SynchronizationBlockBase> synchronizationBlocksPerLoop = new List<SynchronizationBlockBase>();

            foreach (SynchronizationBlockBase synchronizationBlock in _synchronizationBlocks.GetConsumingEnumerable(ct))
            {
                ulong lastBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0;
                if (lastBlockHeight + 1 > synchronizationBlock.BlockHeight)
                {
                    continue;
                }

                if (synchronizationBlock is SynchronizationProducingBlock synchronizationProducingBlock)
                {
                    int ratingPosition = _nodesRatingProvider.GetCandidateRating(_accountState.AccountKey);

                    if (lastBlockHeight + 1 == synchronizationProducingBlock.BlockHeight)
                    {
                        //RetransmitSynchronizationBlock(synchronizationBlockV1);

                        //TODO: this is temporary stub. Need replace with actual implementation after POC
                        if ((synchronizationProducingBlock.Round + 1) % _nodesRatingProvider.GetParticipantsCount() == ratingPosition)
                        {
                            BroadcastConfirmation(synchronizationProducingBlock.BlockHeight, synchronizationProducingBlock.Round, synchronizationProducingBlock.HashPrev, synchronizationProducingBlock.SyncBlockHeight, synchronizationProducingBlock.PowHash);
                        }
                    }
                    else
                    {
                        //TODO: need to understand what to do in case when confirmed synchronization block was not updated yet but next generation synchronization blocks already started to arrive
                    }
                }

                if (synchronizationBlock is SynchronizationBlockRetransmissionV1 synchronizationBlockRetransmission)
                {
                    _retransmittedBlocks.TryAdd(synchronizationBlockRetransmission);

                    if (lastBlockHeight + 1 == synchronizationBlockRetransmission.BlockHeight)
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

                if(!_synchronizationBlocksByHeight[retransmittedBlock.BlockHeight][publicKey].Any(s => s.Signer.Equals(retransmittedBlock.Signer)))
                {
                    _synchronizationBlocksByHeight[retransmittedBlock.BlockHeight][publicKey].Add(retransmittedBlock);
                }

                if(CheckSynchronizationCompleteConsensusAchieved(retransmittedBlock.BlockHeight))
                {
                    //TODO: Round in retransmittedBlock is missing!
                    BroadcastConfirmation(retransmittedBlock.BlockHeight, 0, retransmittedBlock.HashPrev, retransmittedBlock.SyncBlockHeight, retransmittedBlock.PowHash);
                }
            }
        }

        private void BroadcastConfirmation(ulong height, ushort round, byte[] prevHash, ulong syncBlockHeight, byte[] powValue)
        {
            //List<SynchronizationBlockRetransmissionV1> retransmittedSyncBlocks = _synchronizationBlocksByHeight[height].Where(r => _nodeContext.SyncGroupParticipants.Any(p => p.Key == r.Key)).Select(kv => kv.Value.First()).OrderBy(s => s.ConfirmationPublicKey.ToHexString()).ToList();

            SynchronizationConfirmedBlock synchronizationConfirmedBlock = new SynchronizationConfirmedBlock
            {
                BlockHeight = height,
                HashPrev = prevHash, //TODO: does not seems too secure
                SyncBlockHeight = syncBlockHeight, //TODO: does not seems too secure
                PowHash = powValue, //TODO: does not seems too secure
                Round = round,
                PublicKeys = new byte[0][],// retransmittedSyncBlocks.Select(b => b.ConfirmationPublicKey).ToArray(),
                Signatures = new byte[0][] //retransmittedSyncBlocks.Select(b => b.ConfirmationSignature).ToArray()
            };

            ISerializer confirmationBlockSerializer = _signatureSupportSerializersFactory.Create(synchronizationConfirmedBlock);

            _communicationService.PostMessage(_neighborhoodState.GetAllNeighbors(), confirmationBlockSerializer);
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
                if (_nodeContext.SyncGroupParticipants.Any(p => p.Key == publicKey))
                {
                    IEnumerable<SynchronizationBlockRetransmissionV1> lst = retransmittedSyncBlocks[publicKey].Where(s => _nodeContext.SyncGroupParticipants.Any(p => p.Key.Equals(s.ConfirmationPublicKey)));
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
                ConfirmationPublicKey = synchronizationBlockV1.Signer.Value.ToArray(),
                ConfirmationSignature = synchronizationBlockV1.Signature.ToArray(),
                Signer = _accountState.AccountKey
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
