using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Exceptions;
using Wist.Node.Core.Interfaces;

namespace Wist.Node.Core.ConsensusOperations
{
    [RegisterDefaultImplementation(typeof(IConsensusOperationFactory), Lifetime = LifetimeManagement.Singleton)]
    public class ConsensusOperationFactory : IConsensusOperationFactory
    {
        private readonly Dictionary<ChainType, SortedList<ushort, Stack<IConsensusOperation>>> _consensusOperations;

        public ConsensusOperationFactory(IConsensusOperation[] consensusOperations)
        {
            _consensusOperations = new Dictionary<ChainType, SortedList<ushort, Stack<IConsensusOperation>>>();
            
            foreach (IConsensusOperation consensusOperation in consensusOperations)
            {
                if(!_consensusOperations.ContainsKey(consensusOperation.ChainType))
                {
                    _consensusOperations.Add(consensusOperation.ChainType, new SortedList<ushort, Stack<IConsensusOperation>>());
                }

                if (!_consensusOperations[consensusOperation.ChainType].ContainsKey(consensusOperation.Priority))
                {
                    _consensusOperations[consensusOperation.ChainType].Add(consensusOperation.Priority, new Stack<IConsensusOperation>());
                }

                if(_consensusOperations[consensusOperation.ChainType][consensusOperation.Priority].Count == 0)
                {
                    _consensusOperations[consensusOperation.ChainType][consensusOperation.Priority].Push(consensusOperation);
                }
            }
        }

        public IConsensusOperation GetNextOperation(ChainType chainType, IConsensusOperation prevOperation = null)
        {
            if(!_consensusOperations.ContainsKey(chainType))
            {
                throw new ConsensusOnChainTypeNotSupportedException(chainType);
            }

            IConsensusOperation consensusOperation = null;
            Stack<IConsensusOperation> consensusOperationsStack = null;
            if (prevOperation == null)
            {
                consensusOperationsStack = _consensusOperations[chainType][_consensusOperations[chainType].Keys[0]];
            }
            else
            {
                int index = _consensusOperations[chainType].IndexOfKey(prevOperation.Priority) + 1;
                if (_consensusOperations[chainType].Keys.Count > index)
                {
                    consensusOperationsStack = _consensusOperations[chainType][_consensusOperations[chainType].Keys[index]];
                }
            }

            if (consensusOperationsStack != null)
            {
                if (consensusOperationsStack.Count > 1)
                {
                    consensusOperation = consensusOperationsStack.Pop();
                }
                else
                {
                    IConsensusOperation template = consensusOperationsStack.Pop();
                    consensusOperation = (IConsensusOperation)ServiceLocator.Current.GetInstance(template.GetType());
                    consensusOperationsStack.Push(template);
                }
            }

            return consensusOperation;
        }

        public void Utilize(IConsensusOperation operation)
        {
            if (!_consensusOperations.ContainsKey(operation.ChainType))
            {
                throw new ConsensusOnChainTypeNotSupportedException(operation.ChainType);
            }

            if (_consensusOperations[operation.ChainType].ContainsKey(operation.Priority))
            {
                Stack<IConsensusOperation> consensusOperationsStack = _consensusOperations[operation.ChainType][operation.Priority];

                consensusOperationsStack.Push(operation);
            }
        }
    }
}
