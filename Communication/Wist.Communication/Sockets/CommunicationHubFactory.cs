using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Communication.Interfaces;

namespace Wist.Communication.Sockets
{
    public class CommunicationHubFactory : ICommunicationHubFactory
    {
        private readonly Stack<ICommunicationHub> _communicationHubPool;
        public CommunicationHubFactory()
        {
            _communicationHubPool = new Stack<ICommunicationHub>();
        }

        public ICommunicationHub Create()
        {
            if(_communicationHubPool.Count > 0)
            {
                return _communicationHubPool.Pop();
            }

            return ServiceLocator.Current.GetInstance<ICommunicationHub>();
        }

        public void Utilize(ICommunicationHub communicationHub)
        {
            _communicationHubPool.Push(communicationHub);
        }
    }
}
