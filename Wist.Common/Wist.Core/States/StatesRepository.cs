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

        public StatesRepository(IState[] states)
        {
            _states = new Dictionary<string, IState>();

            foreach (IState state in states)
            {
                if(!_states.ContainsKey(state.Name))
                {
                    _states.Add(state.Name, state);
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

        public T GetInstance<T>() where T : class, IState
        {
            if(!_states.ContainsKey(typeof(T).Name))
            {
                throw new StateServiceNotSupportedException(typeof(T).FullName);
            }

            return (T)_states[typeof(T).Name];
        }
    }
}
