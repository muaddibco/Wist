using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.Consensus
{
    [RegisterDefaultImplementation(typeof(IConsensusCheckingService), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusCheckingService : IConsensusCheckingService
    {
        private readonly ConcurrentDictionary<BlockBase, BlockConsensusState> _blockConsensusStatesMap;
        private readonly ConcurrentQueue<BlockConsensusState> _blockConsensusToCheck;
        private readonly IChainDataServicesManager _chainDataServicesManager;
        private readonly IConsensusHub _consensusHub;

        public ConsensusCheckingService(IChainDataServicesManager chainDataServicesManager, IConsensusHub consensusHub)
        {
            _chainDataServicesManager = chainDataServicesManager;
            _consensusHub = consensusHub;
            _blockConsensusStatesMap = new ConcurrentDictionary<BlockBase, BlockConsensusState>();
            _blockConsensusToCheck = new ConcurrentQueue<BlockConsensusState>();
        }

        public void EnrollConsensusDecisions(BlockBase block, IEnumerable<ValidationDecision> consensusDecisions)
        {
            BlockConsensusState consensusState;

            if (!_blockConsensusStatesMap.ContainsKey(block))
            {
                consensusState = new BlockConsensusState(block, consensusDecisions.ToDictionary(c => c.Participant.PublicKey.ToHexString(), c => c.State));
                _blockConsensusStatesMap.TryAdd(block, consensusState);
            }
            else if (_blockConsensusStatesMap.TryGetValue(block, out consensusState))
            {
                lock (consensusState)
                {
                    foreach (var consensusDecision in consensusDecisions.Where(d => !consensusState.ParticipantDecisionsMap.ContainsKey(d.Participant.PublicKey.ToHexString())))
                    {
                        consensusState.IsChecked = false;
                        consensusState.ParticipantDecisionsMap.Add(consensusDecision.Participant.PublicKey.ToHexString(), consensusDecision.State);
                    }
                }

                if (!consensusState.IsChecked && !consensusState.IsConsensusReached)
                {
                    _blockConsensusToCheck.Enqueue(consensusState);
                }
            }
        }

        public void Initialize(CancellationToken ct)
        {
            Task.Factory.StartNew(o =>
            {
                CancellationToken cancellationToken = (CancellationToken)o;
                CheckConsensusDecisions(cancellationToken);
            }, ct, TaskCreationOptions.LongRunning);
        }

        private void CheckConsensusDecisions(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                BlockConsensusState blockConsensusState;
                if (_blockConsensusToCheck.TryDequeue(out blockConsensusState))
                {
                    CheckDecisions(blockConsensusState);
                }
                else
                {
                    Thread.Yield();
                }
            }
        }

        private void CheckDecisions(BlockConsensusState blockConsensusState)
        {
            bool approved;
            bool consensusReached = false;
            lock (blockConsensusState)
            {
                consensusReached = ConsensusReached(blockConsensusState, out approved);
                if (consensusReached)
                {
                    lock (_blockConsensusStatesMap)
                    {
                        BlockConsensusState removedBlockConsensusState;
                        _blockConsensusStatesMap.TryRemove(blockConsensusState.Block, out removedBlockConsensusState);
                    }
                }

                blockConsensusState.IsChecked = true;
                blockConsensusState.IsChecked = consensusReached;
            }

            if (approved)
            {
                IChainDataService chainDataService = _chainDataServicesManager.GetChainDataService(blockConsensusState.Block.PacketType);
                chainDataService.AddBlock(blockConsensusState.Block);
            }
        }

        private bool ConsensusReached(BlockConsensusState blockConsensusState, out bool approved)
        {
            //TODO: refactor for better performance
            int approveWeight = (int)blockConsensusState.ParticipantDecisionsMap.Where(p => p.Value == BlockLattice.Core.Enums.ValidationState.Approved).Select(p => _consensusHub.GroupParticipants[p.Key].Weight).Sum();
            int rejectWeight = (int)blockConsensusState.ParticipantDecisionsMap.Where(p => p.Value == BlockLattice.Core.Enums.ValidationState.Rejected).Select(p => _consensusHub.GroupParticipants[p.Key].Weight).Sum();

            bool isConsensusReached = (double)approveWeight / (double)_consensusHub.TotalWeight >= 0.66 || (double)rejectWeight / (double)_consensusHub.TotalWeight >= 0.66 || (double)(approveWeight + rejectWeight) / (double)_consensusHub.TotalWeight >= 0.9;
            
            approved = isConsensusReached && (approveWeight > rejectWeight);

            return isConsensusReached;
        }
    }
}
