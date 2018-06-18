using CommonServiceLocator;
using System;
using System.Collections.Generic;
using Wist.Communication.Exceptions;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Communication
{
    [RegisterDefaultImplementation(typeof(ICommunicationServicesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class CommunicationServicesRepository : ICommunicationServicesRepository
    {
        private readonly Dictionary<string, ICommunicationService> _communicationServicesPool;

        public CommunicationServicesRepository(ICommunicationService[] communicationServices)
        {
            _communicationServicesPool = new Dictionary<string, ICommunicationService>();

            foreach (ICommunicationService communicationService in communicationServices)
            {
                if(!_communicationServicesPool.ContainsKey(communicationService.Name))
                {
                    _communicationServicesPool.Add(communicationService.Name, communicationService);
                }
            }
        }

        public ICommunicationService GetInstance(string key)
        {
            if(!_communicationServicesPool.ContainsKey(key))
            {
                throw new CommunicationServiceNotSupportedException(key);
            }

            return _communicationServicesPool[key];
        }
    }
}
