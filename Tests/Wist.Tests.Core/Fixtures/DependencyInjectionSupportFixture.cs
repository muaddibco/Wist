using CommonServiceLocator;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Unity.Lifetime;
using Unity.ServiceLocation;
using Wist.Core.Logging;

namespace Wist.Tests.Core.Fixtures
{
    public class DependencyInjectionSupportFixture : IDisposable
    {
        public DependencyInjectionSupportFixture()
        {
            UnityContainer container = new UnityContainer();
            container.RegisterInstance<IUnityContainer>(container, new ContainerControlledLifetimeManager());

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
        }

        public void Dispose()
        {
        }
    }
}
