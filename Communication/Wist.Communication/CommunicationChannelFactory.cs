using CommonServiceLocator;
using Wist.Communication.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wist.Communication
{
    public class CommunicationChannelFactory : ICommunicationChannelFactory
    {
        private readonly Stack<ICommunicationChannel> _handlers;

        public CommunicationChannelFactory()
        {
            _handlers = new Stack<ICommunicationChannel>();
        }

        public ICommunicationChannel Create()
        {
            if(_handlers.Count > 1)
            {
                return _handlers.Pop();
            }

            return ServiceLocator.Current.GetInstance<ICommunicationChannel>();
        }

        public void Utilize(ICommunicationChannel handler)
        {
            _handlers.Push(handler);
        }
    }
}
