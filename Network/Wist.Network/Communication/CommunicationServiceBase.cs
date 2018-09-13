using Unity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Wist.Network.Interfaces;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Communication;
using Wist.Core.Identity;
using Wist.Core.Logging;
using Wist.BlockLattice.Core.Handlers;
using Wist.Network.Topology;

namespace Wist.Network.Communication
{
    public abstract class CommunicationServiceBase : ICommunicationService
    {
        protected readonly ILogger _log;
        private readonly IApplicationContext _applicationContext;
        protected readonly IBufferManagerFactory _bufferManagerFactory;
        protected readonly IPacketsHandler _packetsHandler;
        protected GenericPool<ICommunicationChannel> _communicationChannelsPool;
        protected readonly List<ICommunicationChannel> _clientConnectedList;
        protected int _connectedSockets = 0;
        protected INodesResolutionService _nodesResolutionService;
        protected readonly BlockingCollection<KeyValuePair<IKey, byte[]>> _messagesQueue;
        protected CancellationTokenSource _cancellationTokenSource;
        protected SocketSettings _settings;

        public CommunicationServiceBase(IApplicationContext applicationContext, ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _applicationContext = applicationContext;
            _bufferManagerFactory = bufferManagerFactory;
            _packetsHandler = packetsHandler;
            _clientConnectedList = new List<ICommunicationChannel>();
            _nodesResolutionService = nodesResolutionService;
            _messagesQueue = new BlockingCollection<KeyValuePair<IKey, byte[]>>();
        }
        #region ICommunicationService implementation

        public abstract string Name { get; }

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and 
        /// context objects.  These objects do not need to be preallocated 
        /// or reused, but it is done this way to illustrate how the API can 
        /// easily be used to create reusable objects to increase server performance.
        /// </summary>
        public virtual void Init(SocketSettings settings)
        {
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();

            _settings = settings;

            // Allocates one large byte buffer which all I/O operations use a piece of.  This guards against memory fragmentation
            IBufferManager bufferManager = _bufferManagerFactory.Create();
            bufferManager.InitBuffer(_settings.BufferSize * _settings.MaxConnections * 2, _settings.BufferSize);
            _communicationChannelsPool = new GenericPool<ICommunicationChannel>(_settings.MaxConnections);

            for (int i = 0; i < _settings.MaxConnections; i++)
            {
                ICommunicationChannel communicationChannel = _applicationContext.Container.Resolve<ICommunicationChannel>();
                communicationChannel.SocketClosedEvent += ClientHandler_SocketClosedEvent;
                communicationChannel.Init(bufferManager, _packetsHandler);
                _communicationChannelsPool.Push(communicationChannel);
            }
        }

        public virtual void Start()
        {
            Task.Factory.StartNew(() =>
            {
                ProcessMessagesQueue(_cancellationTokenSource.Token);
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void PostMessage(IKey destination, IPacketProvider message)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _messagesQueue.Add(new KeyValuePair<IKey, byte[]>(destination, message.GetBytes()));
        }

        public void PostMessage(IEnumerable<IKey> destinations, IPacketProvider message)
        {
            if (destinations == null)
            {
                throw new ArgumentNullException(nameof(destinations));
            }

            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            foreach (IKey key in destinations)
            {
                _messagesQueue.Add(new KeyValuePair<IKey, byte[]>(key, message.GetBytes()));
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        #endregion ICommunicationService implementation

        #region Private Functions

        protected abstract Socket CreateSocket();

        private void ClientHandler_SocketClosedEvent(object sender, EventArgs e)
        {
            ICommunicationChannel clientHandler = (ICommunicationChannel)sender;
            _clientConnectedList.Remove(clientHandler);
            ReleaseClientHandler(clientHandler);
        }

        protected virtual void ReleaseClientHandler(ICommunicationChannel communicationChannel)
        {
            if (_clientConnectedList.Contains(communicationChannel))
            {
                _clientConnectedList.Remove(communicationChannel);
            }

            _communicationChannelsPool.Push(communicationChannel);

            Interlocked.Decrement(ref _connectedSockets);
            _log.Info($"Socket with IP {communicationChannel.RemoteIPAddress} disconnected. {_connectedSockets} client(s) connected.");
        }

        protected void InitializeCommunicationChannel(Socket socket)
        {
            ICommunicationChannel communicationChannel = _communicationChannelsPool.Pop();

            Int32 numberOfConnectedSockets = Interlocked.Increment(ref _connectedSockets);
            _log.Info($"Initializing communication channel for IP {socket.LocalEndPoint}, total concurrent accepted sockets is {numberOfConnectedSockets}");

            try
            {
                communicationChannel.AcceptSocket(socket);
                _clientConnectedList.Add(communicationChannel);
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to accept connection by communication channel", ex);
                ReleaseClientHandler(communicationChannel);
            }

        }

        private void ProcessMessagesQueue(CancellationToken token)
        {
            foreach (var messageByKey in _messagesQueue.GetConsumingEnumerable(token))
            {
                IPAddress address = _nodesResolutionService.ResolveNodeAddress(messageByKey.Key);

                ICommunicationChannel communicationChannel = GetChannel(address);

                if (communicationChannel != null)
                {
                    communicationChannel.PostMessage(messageByKey.Value);
                }
            }
        }

        protected ICommunicationChannel GetChannel(IPAddress address)
        {
            //TODO: implement double check with lock for sake of better performance
            lock (_communicationChannelsPool)
            {
                ICommunicationChannel communicationChannel = _clientConnectedList.FirstOrDefault(c => c.RemoteIPAddress.Equals(address));
                if (communicationChannel == null)
                {
                    communicationChannel = CreateChannel(new IPEndPoint(address, _settings.RemotePort));

                    if (communicationChannel != null)
                    {
                        _clientConnectedList.Add(communicationChannel);
                    }
                }

                return communicationChannel;
            }
        }

        protected ICommunicationChannel CreateChannel(EndPoint endPoint)
        {
            ICommunicationChannel communicationChannel = null;

            if (_communicationChannelsPool.Count > 0)
            {
                communicationChannel = _communicationChannelsPool.Pop();

                Interlocked.Increment(ref _connectedSockets);

                Socket socket = CreateSocket();

                SocketAsyncEventArgs socketConnectAsyncEventArgs = new SocketAsyncEventArgs();
                socketConnectAsyncEventArgs.Completed += Connect_Completed;
                socketConnectAsyncEventArgs.RemoteEndPoint = endPoint;
                socketConnectAsyncEventArgs.UserToken = communicationChannel;

                bool willRaiseEvent = socket.ConnectAsync(socketConnectAsyncEventArgs);
                if (!willRaiseEvent)
                {
                    try
                    {
                        if (socketConnectAsyncEventArgs.SocketError == SocketError.Success)
                        {
                            communicationChannel.AcceptSocket(socket);
                        }
                        else
                        {
                            _communicationChannelsPool.Push(communicationChannel);
                        }
                    }
                    finally
                    {
                        socketConnectAsyncEventArgs.Completed -= Connect_Completed;
                    }
                }
            }
            else
            {
                _log.Error("No more room for communication channels left");
            }

            return communicationChannel;
        }

        private void Connect_Completed(object sender, SocketAsyncEventArgs e)
        {
            _log.Info($"Connection to remote endpoint {e.RemoteEndPoint} completed");

            if (e.LastOperation == SocketAsyncOperation.Connect)
            {
                ICommunicationChannel communicationChannel = e.UserToken as ICommunicationChannel;

                if (e.SocketError == SocketError.Success)
                {
                    communicationChannel?.AcceptSocket(e.ConnectSocket);
                }
                else
                {
                    _communicationChannelsPool.Push(communicationChannel);
                }
            }
            else
            {
                throw new ArgumentException("The last operation completed on the socket was not a connect.");
            }
        }

        #endregion Private Functions
    }
}
