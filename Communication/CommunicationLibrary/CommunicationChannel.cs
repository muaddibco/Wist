using DataModel;
using System;
using System.Collections.Generic;

namespace CommunicationLibrary
{
    public class CommunicationChannel : IObservable<MessageBase>
    {
        private readonly List<IObserver<MessageBase>> _observers = new List<IObserver<MessageBase>>();

        public IDisposable Subscribe(IObserver<MessageBase> observer)
        {
            if(!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new Unsubscriber(observer, _observers);
        }

        private class Unsubscriber : IDisposable
        {
            private readonly IObserver<MessageBase> _observer;
            private readonly List<IObserver<MessageBase>> _observers;

            public Unsubscriber(IObserver<MessageBase> observer, List<IObserver<MessageBase>> observers)
            {
                _observer = observer;
                _observers = observers;
            }

            public void Dispose()
            {
                if (_observers.Contains(_observer))
                {
                    _observers.Remove(_observer);
                }
            }
        }
    }
}
