using CommonServiceLocator;
using CommunicationLibrary;
using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Wist.Communication.Tests.Fixtures;
using Xunit;

namespace Wist.Communication.Tests
{
    [Collection("Dependency Injection")]
    public class ClientHandlerTests : IClassFixture<ClientHandlerFixture>
    {
        [Fact]
        public void ParseSingleShortPacket()
        {
            IClientHandler handler = ServiceLocator.Current.GetInstance<IClientHandler>();
            byte[] packet = new byte[] { ClientHandler.DLE, ClientHandler.STX, 0x03, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44};
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc};

            handler.Start();
            handler.PushBuffer(packet, packet.Length);

            Thread.Sleep(100);

            Assert.True(handler.MessagePackets.Count == 1);

            byte[] messagePacket = handler.MessagePackets.Dequeue();
            Assert.Equal(messagePacket, parsedPacket);
        }
    }
}
