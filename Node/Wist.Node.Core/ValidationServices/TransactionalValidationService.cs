using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.DataModel.Transactional;
using Wist.BlockLattice.Core.Enums;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Core.States;
using Wist.Node.Core.Interfaces;
using Wist.Node.Core.Model;

namespace Wist.Node.Core.ValidationServices
{
    [RegisterExtension(typeof(IChainValidationService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalValidationService : IChainValidationService
    {
        public PacketType ChainType => PacketType.TransactionalChain;

        private readonly ManualResetEventSlim _messageTrigger;
        private readonly ConcurrentQueue<TransactionalBlockBase> _blocksAwaiting;
        private readonly HashSet<string> _blocksBeingProcessed;

        private IConsumeValidationReport _reportConsensus;
        private readonly Dictionary<string, Task> _consensusTasks;
        private readonly IValidationOperationFactory _consensusOperationFactory;
        private readonly INodeContext _nodeContext;
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, ValidationState>>> _consensusMap;

        public TransactionalValidationService(IValidationOperationFactory consensusOperationFactory, IStatesRepository statesRepository)
        {
            _messageTrigger = new ManualResetEventSlim();
            _consensusOperationFactory = consensusOperationFactory;
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _consensusMap = new Dictionary<string, Dictionary<string, Dictionary<string, ValidationState>>>();
        }

        public void EnrollForConsensus(BlockBase block)
        {
            // Flow will as follows:
            // 1. Check whether processed block is the only block at the same order received from the same sender?
            // 2. Check whether processed block is block right after last
            // 3. Context-dependent data (funds for Transfer and Accept) validation
            // 4. Once local decision was made it will be retranslated to other nodes in group and their decisions will be accepted
            // 5. Block, that received majority of votes will be stored to local storage, that means it is correct block

            TransactionalBlockBase transactionalBlock = (TransactionalBlockBase)block;

            string originalHash = transactionalBlock.OriginalHash.ToHexString();
            string nbackHash = transactionalBlock.NBackHash.ToHexString();
            string hashOfBody = transactionalBlock.Hash.ToHexString();

            Dictionary<string, Dictionary<string, ValidationState>> nbackHashToConsensusMap = null;
            Dictionary<string, ValidationState> bodyHashToConsensusMap = null;

            if (!_consensusMap.ContainsKey(originalHash))
            {
                nbackHashToConsensusMap = new Dictionary<string, Dictionary<string, ValidationState>>();
                _consensusMap.Add(originalHash, nbackHashToConsensusMap);
            }
            else
            {
                nbackHashToConsensusMap = _consensusMap[originalHash];
            }

            if (!nbackHashToConsensusMap.ContainsKey(nbackHash))
            {
                bodyHashToConsensusMap = new Dictionary<string, ValidationState>();
                nbackHashToConsensusMap.Add(nbackHash, bodyHashToConsensusMap);
            }
            else
            {
                bodyHashToConsensusMap = nbackHashToConsensusMap[nbackHash];
            }

            if (!bodyHashToConsensusMap.ContainsKey(hashOfBody))
            {
                bodyHashToConsensusMap.Add(hashOfBody, ValidationState.Undefined);

                _blocksAwaiting.Enqueue(transactionalBlock);
            }

            _messageTrigger.Set();
        }

        public bool IsBlockEnrolled(BlockBase block)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IConsumeValidationReport reportConsensus, CancellationToken cancellationToken)
        {
            _reportConsensus = reportConsensus;
            Task.Factory.StartNew(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _messageTrigger.Reset();
                    FetchAndProcess(cancellationToken);
                    _messageTrigger.Wait(cancellationToken);
                }
            }, TaskCreationOptions.LongRunning);

            PeriodicTaskFactory.Start(() =>
            {
                if (_blocksAwaiting.Count > 100)
                {
                    Task.Factory.StartNew(() => FetchAndProcess(cancellationToken), TaskCreationOptions.LongRunning);
                }
            }, 1000, cancelToken: cancellationToken, delayInMilliseconds: 3000);
        }

        private void DoProcess(CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                _messageTrigger.Reset();
                FetchAndProcess(cancellationToken);
                _messageTrigger.Wait();
            }
        }

        private void FetchAndProcess(CancellationToken cancellationToken)
        {
            BlockBase block = null;
            while (!cancellationToken.IsCancellationRequested && (block = FetchNextBlock(cancellationToken)) != null)
            {
                Task.Run(async () => 
                {
                    IValidationOperation consensusOperation = null;
                    ValidationState consensusState = ValidationState.Undefined;

                    while ((consensusOperation = _consensusOperationFactory.GetNextOperation(ChainType, consensusOperation)) != null)
                    {
                        if (!await consensusOperation.Validate(block))
                        {
                            consensusState = ValidationState.Rejected;
                            break;
                        }
                    }

                    _reportConsensus.OnValidationReport(block, new ValidationDecision[1] { new ValidationDecision() { Participant = _nodeContext.ThisNode, State = consensusState } });
                });
            }
        }

        private BlockBase FetchNextBlock(CancellationToken cancellationToken)
        {
            lock(_blocksAwaiting)
            {
                while (_blocksAwaiting.Count > 0)
                {
                    TransactionalBlockBase block;
                    if (_blocksAwaiting.TryDequeue(out block))
                    {
                        string originalHash = block.OriginalHash.ToHexString();

                        if (!_blocksBeingProcessed.Contains(originalHash))
                        {
                            _blocksBeingProcessed.Add(originalHash);
                            return block;
                        }
                        else
                        {
                            _blocksAwaiting.Enqueue(block);
                            if (_blocksAwaiting.Count == 1)
                                return null;
                        }
                    }

                    return null;
                }

                return null;
            }
        }
    }
}
