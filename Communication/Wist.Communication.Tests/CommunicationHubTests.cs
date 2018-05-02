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

namespace Wist.Communication.Tests
{
    public class CommunicationHubTests : IClassFixture<DependencyInjectionFixture>
    {
        [Theory]
        [InlineData(3001)]
        public void ConnectivityBaseTest(int listeningPort)
        {
            IClientHandler clientHandler = Substitute.For<IClientHandler>();
            ServiceLocator.Current.GetInstance<IUnityContainer>().RegisterInstance<IClientHandler>(clientHandler);

            IBufferManager bufferManager = Substitute.For<IBufferManager>();
            CommunicationHub communicationHub = new CommunicationHub(bufferManager);

            IPEndPoint communicationEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listeningPort);
            SocketListenerSettings settings = new SocketListenerSettings(1, 1, 1, 100, 2, communicationEndPoint, false);

            communicationHub.Init(settings);
            communicationHub.StartListen();

            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(communicationEndPoint.Address, communicationEndPoint.Port);

            Assert.True(tcpClient.Connected);
        }
    }
}
