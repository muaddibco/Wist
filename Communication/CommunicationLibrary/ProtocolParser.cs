using CommunicationLibrary.Exceptions;
using CommunicationLibrary.Interfaces;
using CommunicationLibrary.Messages;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.Aspects;
using Wist.Core.ExtensionMethods;

namespace CommunicationLibrary
{
    [AutoLog]
    [RegisterExtension(typeof(IProtocolParser), Lifetime = LifetimeManagement.Singleton)]
    public class ProtocolParser : IProtocolParser
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(ProtocolParser));

        private readonly IMessageFactory[] _responseFactories;

        public const byte STX = 0x02;
        public const byte ETX = 0x03;
        public const byte DLE = 0x10;

        private List<ArraySegment<byte>> _buffer;

        public ProtocolParser(IMessageFactory[] responseFactories)
        {
            _responseFactories = responseFactories;

            List<IMessageFactory> distinctFactories = _responseFactories.Distinct(new ResponseFactoryEqualityComparer()).ToList();
            int numberOfDistinctFactories = distinctFactories.Count();

            if (numberOfDistinctFactories != _responseFactories.Length)
            {
                IEnumerable<IMessageFactory> repeatedFactories = _responseFactories.Except(distinctFactories);

                string repeatedFactoriesOpCodes =
                    repeatedFactories.Select(f => f.MessageCode.ToString()).Aggregate((t1, t2) => $"{t1}, {t2}");

                throw new NotUniqueResponseFactoriesException(repeatedFactoriesOpCodes);
            }
        }

        public void PushBuffer(byte[] buf)
        {
            _buffer.Add(new ArraySegment<byte>(buf));
        }

        public IEnumerable<RawMessage> FetchAllMessages()
        {

        }

        public IMessage Parse(byte[] input, out int start, out int end)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug($"Parse: {input.ToHexString()}");
            }

            end = 0;

            for (start = 0; start < input.Length - 1; start++)
            {
                if (input[start] == DLE && input[start + 1] == STX)
                {
                    _log.Debug("Packet start found");
                    break;
                }
            }

            if (start > input.Length - 5)
                return null;

            for (int i = start; i < input.Length - 1; i++)
            {
                if (input[i] == DLE && input[i + 1] == ETX)
                {
                    end = i;
                    _log.Debug("Packet end found");
                    break;
                }
            }

            if (end == 0)
            {
                _log.Debug("Packet end not found");
                return null;
            }

            byte[] packet = new byte[end - start + 2];
            Array.Copy(input, start, packet, 0, end - start + 2);
            _log.Debug($"Packet after parsing {packet.ToHexString()}");

            byte[] packetDecoded = DecodePacket(packet);
            _log.Debug($"Packet after decoding {packetDecoded.ToHexString()}");

            byte opCode = packetDecoded[2];
            int bodyLength = BitConverter.ToInt32(packetDecoded, 3);

            IMessageFactory responseFactory = _responseFactories.FirstOrDefault(r => r.MessageCode == opCode);
            if (responseFactory == null)
            {
                _log.Warn($"Factory for response with opcode 0x{opCode.ToString("X2")} not found");
                return null;
            }

            _log.Debug($"Factory for response with opcode 0x{opCode.ToString("X2")} found");

            byte[] bodyBytes = new byte[bodyLength];

            Array.Copy(packetDecoded, 7, bodyBytes, 0, bodyLength);
            _log.Debug($"Body extracted from packet {bodyBytes.ToHexString()}");

            try
            {
                IMessage response = responseFactory.CreateResponse(bodyBytes);
                _log.Debug($"Response with OpCode {response?.MessageCode.ToString("X2") ?? "null"} obtained from factory is {response?.GetType().Name ?? "null"}");
                return response;
            }
            catch (Exception ex)
            {
                _log.Fatal($"Response creation for opcode 0x{opCode.ToString("X2")} failed", ex);
                return null;
            }
        }

        private byte[] DecodePacket(byte[] packet)
        {
            using (MemoryStream memoryStream = new MemoryStream(packet.Length))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(packet[0]);
                    binaryWriter.Write(packet[1]);
                    for (int i = 2; i < packet.Length - 2; i++)
                    {
                        byte b = packet[i];
                        if (b == DLE)
                        {
                            i++;
                            b = packet[i];
                            binaryWriter.Write((byte)(b - DLE));
                        }
                        else
                            binaryWriter.Write(b);
                    }
                    binaryWriter.Write(packet[packet.Length - 2]);
                    binaryWriter.Write(packet[packet.Length - 1]);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
