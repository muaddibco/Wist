﻿using CommonServiceLocator;
using Wist.Communication;
using Wist.Communication.Interfaces;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Wist.Communication.Tests.Fixtures;
using Xunit;
using Wist.BlockLattice.Core.Interfaces;

namespace Wist.Communication.Tests
{
    public class ClientHandlerTests : IClassFixture<DependencyInjectionFixture>
    {
        public ClientHandlerTests(DependencyInjectionFixture dependencyInjectionFixture)
        {
            DependencyInjectionFixture = dependencyInjectionFixture;
        }

        public DependencyInjectionFixture DependencyInjectionFixture { get; }

        [Fact]
        public void ParseSingleShortPacket()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();  
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel();
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet = new byte[] { CommunicationChannel.DLE, CommunicationChannel.STX, 0x03, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44};
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc};

            handler.PushForParsing(packet, packet.Length);

            Thread.Sleep(100);

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleShortExactPacket()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel();
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet = new byte[] { CommunicationChannel.DLE, CommunicationChannel.STX, 0x03, 0x00, 0xaa, 0xbb, 0xcc};
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc };

            handler.PushForParsing(packet, packet.Length);

            Thread.Sleep(100);

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleShortPacketWithDLE()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel();
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet = new byte[] { CommunicationChannel.DLE, CommunicationChannel.STX, CommunicationChannel.DLE, CommunicationChannel.DLE + 0x02, 0x00, 0xaa, 0xbb, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb };

            handler.PushForParsing(packet, packet.Length);

            Thread.Sleep(100);

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleLongPacket()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel();
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet1 = new byte[] { CommunicationChannel.DLE, CommunicationChannel.STX, 0x09, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] packet2 = new byte[] { 0x03, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0x44, 0x03, 0x00, 0xaa, 0xbb };

            handler.PushForParsing(packet1, packet1.Length);
            handler.PushForParsing(packet2, packet2.Length);

            Thread.Sleep(100);

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }

        [Fact]
        public void ParseSingleLongPacketDleIsLast()
        {
            List<byte[]> packets = new List<byte[]>();
            IPacketsHandler messagesHandler = Substitute.For<IPacketsHandler>();
            messagesHandler.WhenForAnyArgs(m => m.Push(null)).Do(ci => packets.Add(ci.ArgAt<byte[]>(0)));
            ICommunicationChannel handler = new CommunicationChannel();
            handler.Init(DependencyInjectionFixture.BufferManager, messagesHandler);
            byte[] packet1 = new byte[] { 0x45, 0x65, CommunicationChannel.DLE };
            byte[] packet2 = new byte[] { CommunicationChannel.STX, 0x09, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] packet3 = new byte[] { 0x03, 0x00, 0xaa, 0xbb, 0xcc, 0xdd, 0x44 };
            byte[] parsedPacket = new byte[] { 0xaa, 0xbb, 0xcc, 0xdd, 0x44, 0x03, 0x00, 0xaa, 0xbb };

            handler.PushForParsing(packet1, packet1.Length);
            handler.PushForParsing(packet2, packet2.Length);
            handler.PushForParsing(packet3, packet3.Length);

            Thread.Sleep(100);

            Assert.True(packets.Count == 1);

            byte[] messagePacket = packets.First();
            Assert.Equal(messagePacket, parsedPacket);
        }
    }
}
