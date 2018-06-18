using CommonServiceLocator;
using Wist.Communication.Interfaces;
using Wist.Communication.Sockets;
using NSubstitute;
using System;
using System.Net;
using System.Net.Sockets;
using Unity;
using Wist.Communication.Tests.Fixtures;
using Xunit;
using Xunit.Sdk;
using Wist.Core.Logging;

namespace Wist.Communication.Tests
{
    public class CommunicationHubTests : IClassFixture<DependencyInjectionFixture>
    {
        [Theory]
        [InlineData(3001)]
        public void ConnectivityBaseTest(int listeningPort)
        {
            ICommunicationChannel clientHandler = Substitute.For<ICommunicationChannel>();
            ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance(clientHandler);

            ILoggerService loggerService = Substitute.For<ILoggerService>();

            IBufferManagerFactory bufferManagerFactory = Substitute.For<IBufferManagerFactory>();
            CommunicationServiceBase communicationHub = new TcpCommunicationService(loggerService, bufferManagerFactory, null, null);

            IPEndPoint communicationEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listeningPort);
            SocketListenerSettings settings = new SocketListenerSettings(1, 100, communicationEndPoint);

            communicationHub.Init(settings, null);
            communicationHub.Start();

            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(communicationEndPoint.Address, communicationEndPoint.Port);

            Assert.True(tcpClient.Connected);
        }

        //TODO: 
        // CommunicationProvisioning tests
    }
}
