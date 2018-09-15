using CommonServiceLocator;
using System.Collections.Generic;
using Wist.BlockLattice.Core.Enums;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Node.Core.Exceptions;

namespace Wist.Node.Core.ValidationOperations
{
    [RegisterDefaultImplementation(typeof(IValidationOperationFactory), Lifetime = LifetimeManagement.Singleton)]
    public class ValidationOperationFactory : IValidationOperationFactory
    {
        private readonly Dictionary<PacketType, SortedList<ushort, Stack<IValidationOperation>>> _consensusOperations;

        public ValidationOperationFactory(IValidationOperation[] consensusOperations)
        {
            _consensusOperations = new Dictionary<PacketType, SortedList<ushort, Stack<IValidationOperation>>>();
            
            foreach (IValidationOperation consensusOperation in consensusOperations)
            {
                if(!_consensusOperations.ContainsKey(consensusOperation.ChainType))
                {
                    _consensusOperations.Add(consensusOperation.ChainType, new SortedList<ushort, Stack<IValidationOperation>>());
                }

                if (!_consensusOperations[consensusOperation.ChainType].ContainsKey(consensusOperation.Priority))
                {
                    _consensusOperations[consensusOperation.ChainType].Add(consensusOperation.Priority, new Stack<IValidationOperation>());
                }

                if(_consensusOperations[consensusOperation.ChainType][consensusOperation.Priority].Count == 0)
                {
                    _consensusOperations[consensusOperation.ChainType][consensusOperation.Priority].Push(consensusOperation);
                }
            }
        }

        public IValidationOperation GetNextOperation(PacketType chainType, IValidationOperation prevOperation = null)
        {
            if(!_consensusOperations.ContainsKey(chainType))
            {
                throw new ConsensusOnChainTypeNotSupportedException(chainType);
            }

            IValidationOperation consensusOperation = null;
            Stack<IValidationOperation> consensusOperationsStack = null;
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
                    IValidationOperation template = consensusOperationsStack.Pop();
                    consensusOperation = (IValidationOperation)ServiceLocator.Current.GetInstance(template.GetType());
                    consensusOperationsStack.Push(template);
                }
            }

            return consensusOperation;
        }

        public void Utilize(IValidationOperation operation)
        {
            if (!_consensusOperations.ContainsKey(operation.ChainType))
            {
                throw new ConsensusOnChainTypeNotSupportedException(operation.ChainType);
            }

            if (_consensusOperations[operation.ChainType].ContainsKey(operation.Priority))
            {
                Stack<IValidationOperation> consensusOperationsStack = _consensusOperations[operation.ChainType][operation.Priority];

                consensusOperationsStack.Push(operation);
            }
        }
    }
}
