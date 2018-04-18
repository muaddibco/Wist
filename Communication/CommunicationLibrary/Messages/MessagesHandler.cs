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
    [RegisterDefaultImplementation(typeof(IMessagesHandler), Lifetime = LifetimeManagement.Singleton)]
    public class MessagesHandler : IMessagesHandler
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(MessagesHandler));
        private readonly ConcurrentQueue<byte[]> _messagePackets;
        private readonly ConcurrentQueue<MessageErrorPacket> _messageErrorPackets;
        private CancellationTokenSource _cancellationTokenSource;
        private ManualResetEventSlim _messageTrigger;
        private ManualResetEventSlim _messageErrorTrigger;

        public MessagesHandler()
        {
            _messagePackets = new ConcurrentQueue<byte[]>();
            _messageErrorPackets = new ConcurrentQueue<MessageErrorPacket>();
            _messageTrigger = new ManualResetEventSlim();
            _messageErrorTrigger = new ManualResetEventSlim();
        }

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
                Task.Factory.StartNew(() => ProcessMessageErrorPackets(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
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
                MessageBase msg;
                using (MemoryStream ms = new MemoryStream(messagePacket))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        ushort messageType = br.ReadUInt16();
                        ushort length = br.ReadUInt16();

                        if(length == 0)
                        {
                            PushMessageErrorPacket(new MessageErrorPacket(messagePacket, MessageErrors.LENGTH_IS_INVALID));
                            continue;
                        }

                        int actualMessageBodyLength = messagePacket.Length - 4 - 32 - 64;
                        if (actualMessageBodyLength != length)
                        {
                            PushMessageErrorPacket(new MessageErrorPacket(messagePacket, MessageErrors.LENGTH_DOES_NOT_MATCH));
                            continue;
                        }

                        byte[] messageBody = br.ReadBytes(length);
                        byte[] signature = br.ReadBytes(64);
                        byte[] publickKey = br.ReadBytes(32);

                        if(!VerifySignature(messageBody, signature, publickKey))
                        {
                            PushMessageErrorPacket(new MessageErrorPacket(messagePacket, MessageErrors.SIGNATURE_IS_INVALID));
                            continue;
                        }
                    }
                }
            }
        }

        private bool VerifySignature(byte[] messageBody, byte[] signature, byte[] publicKey)
        {
            return true;
        }

        private void PushMessageErrorPacket(MessageErrorPacket messageErrorPacket)
        {
            _messageErrorPackets.Enqueue(messageErrorPacket);
            _messageErrorTrigger.Set();
        }

        private void ProcessMessageErrorPackets(CancellationToken ct)
        {
            while(!ct.IsCancellationRequested)
            {
                MessageErrorPacket messageErrorPacket;
                if(_messageErrorPackets.TryDequeue(out messageErrorPacket))
                {
                    _messageErrorTrigger.Reset();
                    switch (messageErrorPacket.ErrorCode)
                    {
                        case MessageErrors.LENGTH_IS_INVALID:
                            _log.Error($"Invalid message length 0: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                        case MessageErrors.LENGTH_DOES_NOT_MATCH:
                            _log.Error($"Actual message body length is differs from declared: {messageErrorPacket.MessagePacket.ToHexString()}");
                            break;
                        case MessageErrors.SIGNATURE_IS_INVALID:
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

        public class MessageErrorPacket
        {
            public MessageErrorPacket(byte[] messagePacket, MessageErrors errorCode)
            {
                MessagePacket = messagePacket;
                ErrorCode = errorCode;
            }

            public MessageErrors ErrorCode { get; }
            public byte[] MessagePacket { get; }
        }
    }
}
