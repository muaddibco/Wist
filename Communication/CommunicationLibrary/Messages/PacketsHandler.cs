using CommunicationLibrary.Interfaces;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.Core.ExtensionMethods;
using Wist.Core.Models;

namespace CommunicationLibrary.Messages
{
    [RegisterDefaultImplementation(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class PacketsHandler : IPacketsHandler
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(PacketsHandler));
        private readonly IPacketTypeHandlersFactory _packetTypeHandlersFactory;
        private readonly ConcurrentQueue<byte[]> _messagePackets;
        private CancellationTokenSource _cancellationTokenSource;
        private ManualResetEventSlim _messageTrigger;
        private readonly ConcurrentQueue<PacketErrorMessage> _messageErrorPackets;
        private ManualResetEventSlim _messageErrorTrigger;

        public PacketsHandler(IPacketTypeHandlersFactory packetTypeHandlersFactory)
        {
            _packetTypeHandlersFactory = packetTypeHandlersFactory;
            _messagePackets = new ConcurrentQueue<byte[]>();
            _messageTrigger = new ManualResetEventSlim();
            _messageErrorPackets = new ConcurrentQueue<PacketErrorMessage>();
            _messageErrorTrigger = new ManualResetEventSlim();
        }

        /// <summary>
        /// Bytes being pushed to <see cref="IPacketsHandler"/> must form complete packet for following validation and processing
        /// </summary>
        /// <param name="messagePacket">Bytes of complete message for following processing</param>
        public void Push(byte[] messagePacket)
        {
            _messagePackets.Enqueue(messagePacket);
            _messageTrigger.Set();
        }

        public void Start(bool withErrorsProcessing = true)
        {
            Stop();

            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => ParseMain(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);

            PeriodicTaskFactory.Start(() => 
            {
                if(_messagePackets.Count > 100)
                {
                    Task.Factory.StartNew(() => Parse(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
                }
            }, 1000, cancelToken: _cancellationTokenSource.Token, delayInMilliseconds: 3000);

            if(withErrorsProcessing)
            {
                Task.Factory.StartNew(() => ProcessPacketErrorPackets(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
            }
        }

        public void Stop()
        {
            _messageTrigger?.Set();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        private void ParseMain(CancellationToken token)
        {
            while(!token.IsCancellationRequested)
            {
                _messageTrigger.Reset();
                Parse(token);
                _messageTrigger.Wait();
            }
        }

        private void Parse(CancellationToken token)
        {
            byte[] messagePacket;
            while (!token.IsCancellationRequested && _messagePackets.TryDequeue(out messagePacket))
            {
                Task task = ProcessMessagePacket(messagePacket);
            }
        }

        private async Task ProcessMessagePacket(byte[] messagePacket)
        {
            if(messagePacket == null)
            {
                _log.Warn("An EMPTY packet obtained at ProcessMessagePacket");
                return;
            }

            try
            {
                PacketTypes packetType = (PacketTypes)messagePacket[0];
                IPacketTypeHandler packetTypeHandler = _packetTypeHandlersFactory.Create(packetType);
                PacketErrorMessage packetErrorMessage = await packetTypeHandler.ProcessPacket(messagePacket);
                if (packetErrorMessage.ErrorCode != PacketsErrors.NO_ERROR)
                {
                    PushPacketErrorPacket(packetErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to process packet {messagePacket.ToHexString()}", ex);
            }
        }

        private void PushPacketErrorPacket(PacketErrorMessage messageErrorPacket)
        {
            _messageErrorPackets.Enqueue(messageErrorPacket);
            _messageErrorTrigger.Set();
        }

        private void ProcessPacketErrorPackets(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                PacketErrorMessage messageErrorPacket;
                if (_messageErrorPackets.TryDequeue(out messageErrorPacket))
                {
                    _messageErrorTrigger.Reset();
                    switch (messageErrorPacket.ErrorCode)
                    {
                        case PacketsErrors.LENGTH_IS_INVALID:
                            _log.Error($"Invalid message length 0: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                        case PacketsErrors.LENGTH_DOES_NOT_MATCH:
                            _log.Error($"Actual message body length is differs from declared: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                        case PacketsErrors.SIGNATURE_IS_INVALID:
                            _log.Error($"Message signature is invalid: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                    }
                }
                else
                {
                    _messageErrorTrigger.Wait();
                }
            }
        }
    }
}
