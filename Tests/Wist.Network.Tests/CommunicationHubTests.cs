using CommonServiceLocator;
using Wist.Network.Interfaces;
using Wist.Network.Communication;
using NSubstitute;
using System;
using System.Net;
using System.Net.Sockets;
using Unity;
using Wist.Network.Tests.Fixtures;
using Xunit;
using Xunit.Sdk;
using Wist.Core.Logging;
using Wist.Core.Architecture;

namespace Wist.Network.Tests
{
    public class CommunicationHubTests : IClassFixture<DependencyInjectionFixture>
    {
        [Theory]
        [InlineData(3001)]
        public void ConnectivityBaseTest(int listeningPort)
        {
            ICommunicationChannel clientHandler = Substitute.For<ICommunicationChannel>();
            ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance(clientHandler);

            ApplicationContext applicationContext = new ApplicationContext() { Container = (UnityContainer)ServiceLocator.Current.GetInstance<IUnityContainer>() };

            ILoggerService loggerService = Substitute.For<ILoggerService>();

            IBufferManagerFactory bufferManagerFactory = Substitute.For<IBufferManagerFactory>();
            ServerCommunicationServiceBase communicationHub = new TcpCommunicationService(applicationContext, loggerService, bufferManagerFactory, null, null);

            IPEndPoint communicationEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listeningPort);
            SocketListenerSettings settings = new SocketListenerSettings(1, 100, communicationEndPoint);

            communicationHub.Init(settings);
            communicationHub.Start();

            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(communicationEndPoint.Address, communicationEndPoint.Port);

            Assert.True(tcpClient.Connected);
        }

        //TODO: 
        // CommunicationProvisioning tests
    }
}
