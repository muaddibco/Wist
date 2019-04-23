using System;
using System.Collections.Generic;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Configuration;
using Wist.Core.Exceptions;

namespace Wist.Core.Identity
{

    [RegisterDefaultImplementation(typeof(IIdentityKeyProvidersRegistry), Lifetime = LifetimeManagement.Singleton)]
    public class IdentityKeyProvidersRegistry : IIdentityKeyProvidersRegistry
    {
        public static string TRANSACTIONS_IDENTITY_KEY_PROVIDER_NAME = "TransactionRegistry";

        private readonly Dictionary<string, IIdentityKeyProvider> _identityKeyProviders;
        private readonly IIdentityKeyProvider _currentIdentityKeyProvider;

        public IdentityKeyProvidersRegistry(IConfigurationService configurationService, IIdentityKeyProvider[] identityKeyProviders)
        {
            _identityKeyProviders = new Dictionary<string, IIdentityKeyProvider>();

            foreach (IIdentityKeyProvider item in identityKeyProviders)
            {
                if(!_identityKeyProviders.ContainsKey(item.Name))
                {
                    _identityKeyProviders.Add(item.Name, item);
                }
            }

            IIdentityConfiguration identityConfiguration = configurationService.Get<IIdentityConfiguration>();
            string currentIdentityKeyProviderName = identityConfiguration?.Provider;

            if(string.IsNullOrEmpty(currentIdentityKeyProviderName))
            {
                throw new IdentityConfigurationSectionCorruptedException();
            }

            if(!_identityKeyProviders.ContainsKey(currentIdentityKeyProviderName))
            {
                throw new IdentityProviderNotSupportedException(currentIdentityKeyProviderName);
            }

            _currentIdentityKeyProvider = _identityKeyProviders[currentIdentityKeyProviderName];
        }

        public IIdentityKeyProvider GetInstance()
        {
            return _currentIdentityKeyProvider;
        }

        public IIdentityKeyProvider GetInstance(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (!_identityKeyProviders.ContainsKey(key))
            {
                throw new IdentityProviderNotSupportedException(key);
            }

            return _identityKeyProviders[key];
        }

        public IIdentityKeyProvider GetTransactionsIdenityKeyProvider()
        {
            return GetInstance(TRANSACTIONS_IDENTITY_KEY_PROVIDER_NAME);
        }
    }
}
