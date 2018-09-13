using System.Collections.Generic;
using Wist.Network.Exceptions;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Network.Communication
{
    [RegisterDefaultImplementation(typeof(IServerCommunicationServicesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class ServerCommunicationServicesRepository : IServerCommunicationServicesRepository
    {
        private readonly Dictionary<string, IServerCommunicationService> _communicationServicesPool;

        public ServerCommunicationServicesRepository(IServerCommunicationService[] communicationServices)
        {
            _communicationServicesPool = new Dictionary<string, IServerCommunicationService>();

            foreach (IServerCommunicationService communicationService in communicationServices)
            {
                if(!_communicationServicesPool.ContainsKey(communicationService.Name))
                {
                    _communicationServicesPool.Add(communicationService.Name, communicationService);
                }
            }
        }

        public IServerCommunicationService GetInstance(string key)
        {
            if(!_communicationServicesPool.ContainsKey(key))
            {
                throw new CommunicationServiceNotSupportedException(key);
            }

            return _communicationServicesPool[key];
        }
    }
}
