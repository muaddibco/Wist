using System;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.States
{
    [RegisterDefaultImplementation(typeof(IStatesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class StatesRepository : IStatesRepository
    {
        private readonly Dictionary<string, IState> _states;
        private readonly Dictionary<Type, IState> _statesByTypes;

        public StatesRepository(IState[] states)
        {
            _states = new Dictionary<string, IState>();
            _statesByTypes = new Dictionary<Type, IState>();

            foreach (IState state in states)
            {
                if(!_states.ContainsKey(state.Name))
                {
                    _states.Add(state.Name, state);
                }

                foreach (Type interfaceType in state.GetType().GetInterfaces())
                {
                    if(typeof(IState) != interfaceType && typeof(IState).IsAssignableFrom(interfaceType) && !_statesByTypes.ContainsKey(interfaceType))
                    {
                        _statesByTypes.Add(interfaceType, state);
                    }
                }
            }
        }

        public IState GetInstance(string key)
        {
            if (!_states.ContainsKey(key))
            {
                throw new StateServiceNotSupportedException(key);
            }

            return _states[key];
        }

        public T GetInstance<T>() where T : IState
        {
            if(!_statesByTypes.ContainsKey(typeof(T)))
            {
                throw new StateServiceNotSupportedException(typeof(T).FullName);
            }

            return (T)_statesByTypes[typeof(T)];
        }
    }
}
