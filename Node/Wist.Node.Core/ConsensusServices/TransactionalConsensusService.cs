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
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.ConsensusServices
{
    [RegisterExtension(typeof(IChainConsensusService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionalConsensusService : IChainConsensusService
    {
        public ChainType ChainType => ChainType.TransactionalChain;

        private readonly ManualResetEventSlim _messageTrigger;
        private readonly ConcurrentQueue<TransactionalBlockBase> _blocksAwaiting;
        private readonly HashSet<string> _blocksBeingProcessed;

        private IReportConsensus _reportConsensus;
        private readonly Dictionary<string, Task> _consensusTasks;
        private readonly IConsensusOperationFactory _consensusOperationFactory;
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, ConsensusState>>> _consensusMap;

        public TransactionalConsensusService(IConsensusOperationFactory consensusOperationFactory)
        {
            _messageTrigger = new ManualResetEventSlim();
            _consensusOperationFactory = consensusOperationFactory;
            _consensusMap = new Dictionary<string, Dictionary<string, Dictionary<string, ConsensusState>>>();
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

            Dictionary<string, Dictionary<string, ConsensusState>> nbackHashToConsensusMap = null;
            Dictionary<string, ConsensusState> bodyHashToConsensusMap = null;

            if (!_consensusMap.ContainsKey(originalHash))
            {
                nbackHashToConsensusMap = new Dictionary<string, Dictionary<string, ConsensusState>>();
                _consensusMap.Add(originalHash, nbackHashToConsensusMap);
            }
            else
            {
                nbackHashToConsensusMap = _consensusMap[originalHash];
            }

            if (!nbackHashToConsensusMap.ContainsKey(nbackHash))
            {
                bodyHashToConsensusMap = new Dictionary<string, ConsensusState>();
                nbackHashToConsensusMap.Add(nbackHash, bodyHashToConsensusMap);
            }
            else
            {
                bodyHashToConsensusMap = nbackHashToConsensusMap[nbackHash];
            }

            if (!bodyHashToConsensusMap.ContainsKey(hashOfBody))
            {
                bodyHashToConsensusMap.Add(hashOfBody, ConsensusState.Undefined);

                _blocksAwaiting.Enqueue(transactionalBlock);
            }

            _messageTrigger.Set();
        }

        public bool IsBlockEnrolled(BlockBase block)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IReportConsensus reportConsensus, CancellationToken cancellationToken)
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
                    IConsensusOperation consensusOperation = null;
                    ConsensusState consensusState = ConsensusState.Undefined;

                    while ((consensusOperation = _consensusOperationFactory.GetNextOperation(ChainType, consensusOperation)) != null)
                    {
                        if (!await consensusOperation.Validate(block))
                        {
                            consensusState = ConsensusState.Rejected;
                            break;
                        }
                    }

                    _reportConsensus.OnReportConsensus(block, consensusState);
                });
            }
        }

        private BlockBase FetchNextBlock(CancellationToken cancellationToken)
        {
            lock(_blocksAwaiting)
            {
                while (_blocksAwaiting.Count > 0)
                {
                    BlockBase block;
                    if (_blocksAwaiting.TryDequeue(out block))
                    {
                        TransactionalBlockBase transactionalBlock = (TransactionalBlockBase)block;

                        string originalHash = transactionalBlock.OriginalHash.ToHexString();

                        if (!_blocksBeingProcessed.Contains(originalHash))
                        {
                            _blocksBeingProcessed.Add(originalHash);
                            return transactionalBlock;
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
