using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Registry;
using Wist.BlockLattice.Core.Enums;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.States;
using Wist.Core.Synchronization;

namespace Wist.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistrySyncHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistrySync";

        private readonly Dictionary<byte, RoundDescriptor> _roundDescriptors;
        private readonly BlockingCollection<RegistryBlockBase> _registryBlocks;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IDisposable _syncContextChangedUnsibsciber;
        private readonly object _syncRound = new object();

        private CancellationToken _cancellationToken;
        private byte _round;

        public TransactionsRegistrySyncHandler(IStatesRepository statesRepository)
        {
            _roundDescriptors = new Dictionary<byte, RoundDescriptor>();
            _registryBlocks = new BlockingCollection<RegistryBlockBase>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _syncContextChangedUnsibsciber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)SynchronizationStateChanged));
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _cancellationToken.Register(() => { _syncContextChangedUnsibsciber.Dispose(); });

            Task.Factory.StartNew(() => {
                ProcessBlocks(ct);
            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(BlockBase blockBase)
        {
            RegistryBlockBase registryBlock = blockBase as RegistryBlockBase;

            if (registryBlock is TransactionsFullBlock || registryBlock is TransactionsRegistryConfidenceBlock)
            {
                _registryBlocks.Add(registryBlock);
            }
        }

        #region Private Functions

        private void SynchronizationStateChanged(string propName)
        {
            _round = 0;
        }

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (RegistryBlockBase registryBlock in _registryBlocks.GetConsumingEnumerable(ct))
            {
                TransactionsFullBlock transactionsFullBlock = registryBlock as TransactionsFullBlock;
                TransactionsRegistryConfidenceBlock transactionsRegistryConfidenceBlock = registryBlock as TransactionsRegistryConfidenceBlock;

                if(transactionsFullBlock != null)
                {
                    lock(_syncRound)
                    {
                        if(transactionsFullBlock.Round != _round)
                        {
                            continue;
                        }

                        if(!_roundDescriptors.ContainsKey(_round))
                        {
                            RoundDescriptor roundDescriptor = new RoundDescriptor(new Timer(new TimerCallback(RoundEndedHandler), null, 3000, Timeout.Infinite));
                            roundDescriptor.CandidateBlocks.Add(transactionsFullBlock, 0);
                            _roundDescriptors.Add(_round, roundDescriptor);
                        }
                        else
                        {
                            _roundDescriptors[_round].CandidateBlocks.Add(transactionsFullBlock, 0);
                        }
                    }
                }
                else if(transactionsRegistryConfidenceBlock != null)
                {
                    lock(_syncRound)
                    {
                        if(transactionsRegistryConfidenceBlock.Round != _round)
                        {
                            continue;
                        }

                        if (!_roundDescriptors.ContainsKey(_round))
                        {
                            RoundDescriptor roundDescriptor = new RoundDescriptor(new Timer(new TimerCallback(RoundEndedHandler), null, 3000, Timeout.Infinite));
                            roundDescriptor.VotingBlocks.Add(transactionsRegistryConfidenceBlock);
                            _roundDescriptors.Add(_round, roundDescriptor);
                        }
                        else
                        {
                            _roundDescriptors[_round].VotingBlocks.Add(transactionsRegistryConfidenceBlock);
                        }
                    }
                }
            }
        }

        private void RoundEndedHandler(object state)
        {
            lock(_syncRound)
            {
                RoundDescriptor roundDescriptor = _roundDescriptors[_round];

                
            }
        }

        #endregion Private Functions

        internal class RoundDescriptor
        {
            public RoundDescriptor(Timer timer)
            {
                CandidateBlocks = new Dictionary<TransactionsFullBlock, int>();
                VotingBlocks = new HashSet<TransactionsRegistryConfidenceBlock>();
                Timer = timer;
            }

            internal Dictionary<TransactionsFullBlock, int> CandidateBlocks { get; }
            internal HashSet<TransactionsRegistryConfidenceBlock> VotingBlocks { get; }
            internal Timer Timer { get; }
        }
    }
}
