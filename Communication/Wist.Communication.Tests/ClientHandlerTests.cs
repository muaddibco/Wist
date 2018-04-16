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

        [Fact]
        public void ParseSingleShortExactPacket()
        {
            IClientHandler handler = ServiceLocator.Current.GetInstance<IClientHandler>();
            byte[] packet = new byte[] { ClientHandler.DLE, ClientHandler.STX, 0x03, 0x00, 0xaa, 0xbb, 0xcc};
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc };

            handler.Start();
            handler.PushBuffer(packet, packet.Length);

            Thread.Sleep(100);

            Assert.True(handler.MessagePackets.Count == 1);

            byte[] messagePacket = handler.MessagePackets.Dequeue();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleShortPacketWithDLE()
        {
            IClientHandler handler = ServiceLocator.Current.GetInstance<IClientHandler>();
            byte[] packet = new byte[] { ClientHandler.DLE, ClientHandler.STX, ClientHandler.DLE, ClientHandler.DLE + 0x02, 0x00, 0xaa, 0xbb, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb };

            handler.Start();
            handler.PushBuffer(packet, packet.Length);

            Thread.Sleep(100);

            Assert.True(handler.MessagePackets.Count == 1);

            byte[] messagePacket = handler.MessagePackets.Dequeue();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleLongPacket()
        {
            IClientHandler handler = ServiceLocator.Current.GetInstance<IClientHandler>();
            byte[] packet1 = new byte[] { ClientHandler.DLE, ClientHandler.STX, 0x09, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] packet2 = new byte[] { 0x03, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0x44, 0x03, 0x00, 0xaa, 0xbb };

            handler.Start();
            handler.PushBuffer(packet1, packet1.Length);
            handler.PushBuffer(packet2, packet2.Length);

            Thread.Sleep(100);

            Assert.True(handler.MessagePackets.Count == 1);

            byte[] messagePacket = handler.MessagePackets.Dequeue();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleLongPacketDleIsLast()
        {
            IClientHandler handler = ServiceLocator.Current.GetInstance<IClientHandler>();
            byte[] packet1 = new byte[] { 0x45, 0x65, ClientHandler.DLE };
            byte[] packet2 = new byte[] { ClientHandler.STX, 0x09, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] packet3 = new byte[] { 0x03, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0x44, 0x03, 0x00, 0xaa, 0xbb };

            handler.Start();
            handler.PushBuffer(packet1, packet1.Length);
            handler.PushBuffer(packet2, packet2.Length);
            handler.PushBuffer(packet3, packet3.Length);

            Thread.Sleep(100);

            Assert.True(handler.MessagePackets.Count == 1);

            byte[] messagePacket = handler.MessagePackets.Dequeue();
            Assert.Equal(messagePacket, parsedPacket);
        }
    }
}
