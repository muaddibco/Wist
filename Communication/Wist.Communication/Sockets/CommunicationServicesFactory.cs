using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Wist.Communication.Exceptions;
using Wist.Communication.Interfaces;

namespace Wist.Communication.Sockets
{
    public class CommunicationServicesFactory : ICommunicationServicesFactory
    {
        private readonly Dictionary<string, Stack<ICommunicationService>> _communicationServicesPool;

        public CommunicationServicesFactory(ICommunicationService[] communicationServices)
        {
            _communicationServicesPool = new Dictionary<string, Stack<ICommunicationService>>();

            foreach (ICommunicationService communicationService in communicationServices)
            {
                if(!_communicationServicesPool.ContainsKey(communicationService.Name))
                {
                    _communicationServicesPool.Add(communicationService.Name, new Stack<ICommunicationService>());
                }

                _communicationServicesPool[communicationService.Name].Push(communicationService);
            }
        }

        public ICommunicationService Create(string key)
        {
            if(!_communicationServicesPool.ContainsKey(key))
            {
                throw new CommunicationServiceNotSupportedException(key);
            }

            ICommunicationService communicationService;

            if(_communicationServicesPool[key].Count > 1)
            {
                communicationService = _communicationServicesPool[key].Pop();
            }
            else
            {
                ICommunicationService communicationServiceTemp = _communicationServicesPool[key].Pop();
                communicationService = ServiceLocator.Current.GetInstance<ICommunicationService>(communicationServiceTemp.GetType().FullName);
                _communicationServicesPool[key].Push(communicationServiceTemp);
            }

            return communicationService;
        }

        public void Utilize(ICommunicationService communicationService)
        {
            if (communicationService == null)
            {
                throw new ArgumentNullException(nameof(communicationService));
            }

            if (!_communicationServicesPool.ContainsKey(communicationService.Name))
            {
                throw new CommunicationServiceNotSupportedException(communicationService.Name);
            }

            _communicationServicesPool[communicationService.Name].Push(communicationService);
        }
    }
}
