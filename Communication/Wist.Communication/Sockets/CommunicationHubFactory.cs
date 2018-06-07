using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Communication.Interfaces;

namespace Wist.Communication.Sockets
{
    public class CommunicationHubFactory : ICommunicationHubFactory
    {
        private readonly Stack<ICommunicationServer> _communicationHubPool;
        public CommunicationHubFactory()
        {
            _communicationHubPool = new Stack<ICommunicationServer>();
        }

        public ICommunicationServer Create()
        {
            if(_communicationHubPool.Count > 0)
            {
                return _communicationHubPool.Pop();
            }

            return ServiceLocator.Current.GetInstance<ICommunicationServer>();
        }

        public void Utilize(ICommunicationServer communicationHub)
        {
            _communicationHubPool.Push(communicationHub);
        }
    }
}
