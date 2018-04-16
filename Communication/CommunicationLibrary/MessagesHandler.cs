using CommunicationLibrary.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;

namespace CommunicationLibrary
{
    [RegisterDefaultImplementation(typeof(IMessagesHandler), Lifetime = LifetimeManagement.Singleton)]
    public class MessagesHandler : IMessagesHandler
    {
        private readonly ConcurrentQueue<byte[]> _messagePackets;
        private CancellationTokenSource _cancellationTokenSource;
        private ManualResetEventSlim _messageTrigger;

        public MessagesHandler()
        {
            _messagePackets = new ConcurrentQueue<byte[]>();
        }

        public void Push(byte[] messagePacket)
        {
            _messagePackets.Enqueue(messagePacket);
            _messageTrigger.Set();
        }

        public void Start()
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
                
            }
        }
    }
}
