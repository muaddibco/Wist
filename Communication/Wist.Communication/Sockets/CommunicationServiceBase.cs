﻿using CommonServiceLocator;
using Wist.Communication.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Wist.Core;
using Wist.Core.Architecture;
using Wist.Core.Architecture.Enums;
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using System.Threading.Tasks;
using Wist.Core.Models;
using Wist.Core.Communication;
using System.Collections.Concurrent;
using Wist.Core.Logging;

namespace Wist.Communication.Sockets
{
    public abstract class CommunicationServiceBase : ICommunicationService
    {
        protected readonly ILogger _log;
        private readonly IBufferManagerFactory _bufferManagerFactory;
        private readonly IPacketsHandler _packetsHandler;
        private GenericPool<ICommunicationChannel> _communicationChannelsPool;
        private readonly List<ICommunicationChannel> _clientConnectedList;
        protected SocketListenerSettings _settings;
        protected Socket _listenSocket;
        private int _connectedSockets = 0;
        protected ICommunicationProvisioning _communicationProvisioning = null;
        private INodesResolutionService _nodesResolutionService;
        private readonly BlockingCollection<KeyValuePair<IKey, IPacketProvider>> _messagesQueue;
        private CancellationTokenSource _cancellationTokenSource;
        private Func<ICommunicationChannel, IPEndPoint, int, bool> _onReceiveExtendedValidation;

        /// <summary>
        /// Create an uninitialized server instance.  
        /// To start the server listening for connection requests
        /// call the Init method followed by Start method 
        /// </summary>
        /// <param name="settings">instance of <see cref="SocketListenerSettings"/> with defined settings of listener</param>
        /// <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public CommunicationServiceBase(ILoggerService loggerService, IBufferManagerFactory bufferManagerFactory, IPacketsHandler packetsHandler, INodesResolutionService nodesResolutionService)
        {
            _log = loggerService.GetLogger(GetType().Name);
            _bufferManagerFactory = bufferManagerFactory;
            _packetsHandler = packetsHandler;
            _clientConnectedList = new List<ICommunicationChannel>();
            _nodesResolutionService = nodesResolutionService;
            _messagesQueue = new BlockingCollection<KeyValuePair<IKey, IPacketProvider>>();
        }

        #region ICommunicationService implementation

        public abstract string Name { get; }

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and 
        /// context objects.  These objects do not need to be preallocated 
        /// or reused, but it is done this way to illustrate how the API can 
        /// easily be used to create reusable objects to increase server performance.
        /// </summary>
        public virtual void Init(SocketListenerSettings settings, ICommunicationProvisioning communicationProvisioning = null)
        {
            _cancellationTokenSource?.Cancel();

            _cancellationTokenSource = new CancellationTokenSource();

            _settings = settings;

            if (_communicationProvisioning != null)
            {
                _communicationProvisioning.AllowedEndpointsChanged -= CommunicationProvisioning_AllowedEndpointsChanged;
            }

            _communicationProvisioning = communicationProvisioning;

            if(_communicationProvisioning != null)
            {
                _communicationProvisioning.AllowedEndpointsChanged += CommunicationProvisioning_AllowedEndpointsChanged;
            }

            _packetsHandler?.Initialize();

            // Allocates one large byte buffer which all I/O operations use a piece of.  This guards against memory fragmentation
            IBufferManager bufferManager = _bufferManagerFactory.Create();
            bufferManager.InitBuffer(_settings.ReceiveBufferSize * _settings.MaxConnections * 2, _settings.ReceiveBufferSize);
            _communicationChannelsPool = new GenericPool<ICommunicationChannel>(_settings.MaxConnections);

            for (int i = 0; i < _settings.MaxConnections; i++)
            {
                ICommunicationChannel communicationChannel = ServiceLocator.Current.GetInstance<ICommunicationChannel>();
                communicationChannel.SocketClosedEvent += ClientHandler_SocketClosedEvent;
                communicationChannel.Init(bufferManager, _packetsHandler, _onReceiveExtendedValidation);
                _communicationChannelsPool.Push(communicationChannel);
            }

            Task.Factory.StartNew(() =>
            {
                ProcessMessagesQueue(_cancellationTokenSource.Token);
            }, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void Start()
        {
            _listenSocket = CreateSocket();

            _listenSocket.Bind(_settings.ListeningEndpoint);

            StartAccept();
        }

        protected abstract void StartAccept();

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

            _messagesQueue.Add(new KeyValuePair<IKey, IPacketProvider>(destination, message));
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
                _messagesQueue.Add(new KeyValuePair<IKey, IPacketProvider>(key, message));
            }
        }

        public void Stop()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
        }

        public void RegisterOnReceivedExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceiveExtendedValidation)
        {
            _onReceiveExtendedValidation = onReceiveExtendedValidation;
        }

        #endregion ICommunicationService implementation

        protected abstract Socket CreateSocket();

        #region Private functions

        private void CommunicationProvisioning_AllowedEndpointsChanged(object sender, EventArgs e)
        {
            List<ICommunicationChannel> clientsToBeDisconnected = _clientConnectedList.Where(c => _communicationProvisioning.AllowedEndpoints.All(ep => !ep.Address.Equals(c.RemoteIPAddress))).ToList();

            foreach (ICommunicationChannel clientHandler in clientsToBeDisconnected)
            {
                clientHandler.Close();
            }

            while (_clientConnectedList.Any(c => clientsToBeDisconnected.Contains(c)))
            {
                Thread.Sleep(100);
            };

            foreach (IPEndPoint endPoint in _communicationProvisioning.AllowedEndpoints.Where(ep => _clientConnectedList.All(cc => !cc.RemoteIPAddress.Equals(ep.Address))))
            {
                ICommunicationChannel communicationChannel = CreateChannel(endPoint);
                //TODO: what's next?
            }
        }

        private void ClientHandler_SocketClosedEvent(object sender, EventArgs e)
        {
            ICommunicationChannel clientHandler = (ICommunicationChannel)sender;
            _clientConnectedList.Remove(clientHandler);
            ReleaseClientHandler(clientHandler);
        }

        protected virtual void ReleaseClientHandler(ICommunicationChannel communicationChannel)
        {
            if(_clientConnectedList.Contains(communicationChannel))
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

                ICommunicationChannel communicationChannel = _clientConnectedList.FirstOrDefault(c => c.RemoteIPAddress.Equals(address));

                if (communicationChannel != null)
                {
                    communicationChannel.PostMessage(messageByKey.Value);
                }
                else
                {
                    communicationChannel = CreateChannel(new IPEndPoint(address, _settings.ListeningEndpoint.Port));

                    communicationChannel?.PostMessage(messageByKey.Value);
                }
            }
        }

        private ICommunicationChannel CreateChannel(EndPoint endPoint)
        {
            ICommunicationChannel communicationChannel = null;
            if (_communicationChannelsPool.Count > 0)
            {
                communicationChannel = _communicationChannelsPool.Pop();

                Interlocked.Increment(ref _connectedSockets);

                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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

        #endregion Private functions
    }
}
