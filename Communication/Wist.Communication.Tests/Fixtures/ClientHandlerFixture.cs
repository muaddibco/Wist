using CommonServiceLocator;
using CommunicationLibrary;
using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Unity.Lifetime;
using Unity.ServiceLocation;
using Xunit;

namespace Wist.Communication.Tests.Fixtures
{
    [Collection("Dependency Injection")]
    public class ClientHandlerFixture : IDisposable
    {
        public ClientHandlerFixture()
        {
            UnityContainer container = new UnityContainer();
            container.RegisterInstance<IUnityContainer>(container, new ContainerControlledLifetimeManager());

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));

            container.RegisterType<IClientHandler, ClientHandler>(new Unity.Lifetime.PerResolveLifetimeManager());
        }

        public void Dispose()
        {
        }
    }
}
