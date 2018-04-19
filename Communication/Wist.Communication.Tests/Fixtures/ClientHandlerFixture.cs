using CommonServiceLocator;
using CommunicationLibrary;
using CommunicationLibrary.Interfaces;
using CommunicationLibrary.Sockets;
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
    public class DependencyInjectionFixture : IDisposable
    {
        public DependencyInjectionFixture()
        {
            UnityContainer container = new UnityContainer();
            container.RegisterInstance<IUnityContainer>(container, new ContainerControlledLifetimeManager());

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
            BufferManager = new BufferManager();
            BufferManager.InitBuffer(200, 100);
        }

        public void Dispose()
        {
        }

        public IMessagesHandler MessagesHandler { get; }

        public List<byte[]> Packets { get; }

        public IBufferManager BufferManager { get; set; }
    }
}
