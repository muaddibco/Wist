using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Linq;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Exceptions;

namespace Wist.Core.Cryptography
{
    [RegisterDefaultImplementation(typeof(ISigningServicesRepository), Lifetime = LifetimeManagement.Singleton)]
    public class SigningServicesRepository : ISigningServicesRepository
    {
        private readonly Dictionary<string, Type> _signingServicesTypes;

        public SigningServicesRepository(ISigningService[] signingServices)
        {
            _signingServicesTypes = signingServices?.ToDictionary(s => s.Name, s => s.GetType());
        }

        public ISigningService GetInstance(string key)
        {
            if(!_signingServicesTypes.ContainsKey(key))
            {
                throw new SigningServiceNotSupportedException(key);
            }

            return (ISigningService)ServiceLocator.Current.GetInstance(_signingServicesTypes[key]);
        }
    }
}
