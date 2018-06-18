using System;
using System.Collections.Generic;
using System.Text;
using Wist.Communication.Interfaces;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace Wist.Communication
{
    [RegisterDefaultImplementation(typeof(ICommunicationServicesRegistry), Lifetime = LifetimeManagement.Singleton)]
    public class CommunicationServicesRegistry : ICommunicationServicesRegistry
    {
        private readonly Dictionary<string, ICommunicationService> _communicationServices;

        public CommunicationServicesRegistry()
        {
            _communicationServices = new Dictionary<string, ICommunicationService>();
        }

        public ICommunicationService GetInstance(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //TODO: add key check and dedicated exception
            return _communicationServices[key];
        }

        public void RegisterInstance(ICommunicationService obj, string key)
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
