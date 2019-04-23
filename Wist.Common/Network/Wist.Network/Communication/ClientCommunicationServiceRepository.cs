using System.Collections.Generic;
using Wist.Network.Exceptions;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Network.Communication
{

    [RegisterDefaultImplementation(typeof(IClientCommunicationServiceRepository), Lifetime = LifetimeManagement.Singleton)]
    public class ClientCommunicationServiceRepository : IClientCommunicationServiceRepository
    {
        private readonly Dictionary<string, ICommunicationService> _communicationServices;

        public ClientCommunicationServiceRepository(ICommunicationService[] communicationServices)
        {
            _communicationServices = new Dictionary<string, ICommunicationService>();

            foreach (ICommunicationService communicationService in communicationServices)
            {
                if(!_communicationServices.ContainsKey(communicationService.Name))
                {
                    _communicationServices.Add(communicationService.Name, communicationService);
                }
            }
        }

        public ICommunicationService GetInstance(string key)
        {
            if(!_communicationServices.ContainsKey(key))
            {
                throw new CommunicationServiceNotSupportedException(key);
            }

            return _communicationServices[key];
        }
    }

}
