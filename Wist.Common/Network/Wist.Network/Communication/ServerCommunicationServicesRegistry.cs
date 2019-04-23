using System;
using System.Collections.Generic;
using Wist.Network.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Network.Communication
{
    [RegisterDefaultImplementation(typeof(IServerCommunicationServicesRegistry), Lifetime = LifetimeManagement.Singleton)]
    public class ServerCommunicationServicesRegistry : IServerCommunicationServicesRegistry
    {
        private readonly Dictionary<string, IServerCommunicationService> _communicationServices;

        public ServerCommunicationServicesRegistry()
        {
            _communicationServices = new Dictionary<string, IServerCommunicationService>();
        }

        public IServerCommunicationService GetInstance(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //TODO: add key check and dedicated exception
            return _communicationServices[key];
        }

        public void RegisterInstance(IServerCommunicationService obj, string key)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_communicationServices.ContainsKey(key))
            {
                _communicationServices.Add(key, obj);
            }
        }
    }

}
