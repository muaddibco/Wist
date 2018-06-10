using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Communication.Interfaces;

namespace Wist.Communication.Sockets
{
    public class CommunicationHubFactory : ICommunicationHubFactory
    {
        private readonly Stack<ICommunicationService> _communicationHubPool;
        public CommunicationHubFactory()
        {
            _communicationHubPool = new Stack<ICommunicationService>();
        }

        public ICommunicationService Create()
        {
            if(_communicationHubPool.Count > 0)
            {
                return _communicationHubPool.Pop();
            }

            return ServiceLocator.Current.GetInstance<ICommunicationService>();
        }

        public void Utilize(ICommunicationService communicationHub)
        {
            _communicationHubPool.Push(communicationHub);
        }
    }
}
