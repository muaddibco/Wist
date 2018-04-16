using CommonServiceLocator;
using CommunicationLibrary;
using CommunicationLibrary.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using Unity.Lifetime;
using Unity.ServiceLocation;
using Xunit;

namespace Wist.Communication.Tests.Fixtures
{
    public class ClientHandlerFixture : IDisposable
    {
        public ClientHandlerFixture()
        {
            UnityContainer container = new UnityContainer();
            container.RegisterInstance<IUnityContainer>(container, new ContainerControlledLifetimeManager());

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
        }

        public void Dispose()
        {
        }

        public IMessagesHandler MessagesHandler { get; }

        public List<byte[]> Packets { get; }
    }
}
