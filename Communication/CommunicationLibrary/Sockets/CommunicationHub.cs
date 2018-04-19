using CommonServiceLocator;
using CommunicationLibrary.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Wist.Core;

namespace CommunicationLibrary.Sockets
{
    public class CommunicationHub
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(CommunicationHub));
        private readonly IBufferManager _bufferManager;

        private Semaphore _maxAcceptedClients;
        private GenericPool<SocketAsyncEventArgs> _acceptEventArgsPool;
        private GenericPool<IClientHandler> _clientHandlersPool;
        private SocketListenerSettings _settings;
        private Socket _listenSocket;
        private int _acceptedSockets = 0;

        /// <summary>
        /// Create an uninitialized server instance.  
        /// To start the server listening for connection requests
        /// call the Init method followed by Start method 
        /// </summary>
        /// <param name="settings">instance of <see cref="SocketListenerSettings"/> with defined settings of listener</param>
        /// <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public CommunicationHub(IBufferManager bufferManager)
        {
            _bufferManager = bufferManager;
        }

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and 
        /// context objects.  These objects do not need to be preallocated 
        /// or reused, but it is done this way to illustrate how the API can 
        /// easily be used to create reusable objects to increase server performance.
        /// </summary>
        public void Init(SocketListenerSettings settings)
        {
            _settings = settings;
            
            // Allocates one large byte buffer which all I/O operations use a piece of.  This guards against memory fragmentation
            _bufferManager.InitBuffer(_settings.ReceiveBufferSize * _settings.MaxConnections * _settings.OpsToPreAllocate, _settings.ReceiveBufferSize);
            _acceptEventArgsPool = new GenericPool<SocketAsyncEventArgs>(_settings.MaxSimultaneousAcceptOps);
            _clientHandlersPool = new GenericPool<IClientHandler>(_settings.MaxConnections);
            _maxAcceptedClients = new Semaphore(_settings.MaxConnections, _settings.MaxConnections);

            for (Int32 i = 0; i < _settings.MaxSimultaneousAcceptOps; i++)
            {
                _acceptEventArgsPool.Push(CreateNewSocketAsyncEventArgs(_acceptEventArgsPool));
            }

            Int32 tokenId;

            for (int i = 0; i < _settings.MaxConnections; i++)
            {
                tokenId = _clientHandlersPool.AssignTokenId() + 1000000;

                IClientHandler clientHandler = ServiceLocator.Current.GetInstance<IClientHandler>();
                clientHandler.SocketClosedEvent += ClientHandler_SocketClosedEvent;
                clientHandler.Init(tokenId, _settings.ReceiveBufferSize, _settings.KeepAlive);
                _clientHandlersPool.Push(clientHandler);
            }
        }

        private void ClientHandler_SocketClosedEvent(object sender, EventArgs e)
        {
            IClientHandler clientHandler = (IClientHandler)sender;
            _clientHandlersPool.Push(clientHandler);

            Interlocked.Decrement(ref _acceptedSockets);

            _log.Info($"Socket with tokenId {clientHandler.TokenId} disconnected. {_acceptedSockets} client(s) connected.");

            _maxAcceptedClients.Release();
        }

        public void StartListen()
        {
            _listenSocket = new Socket(_settings.ListeningEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_settings.ListeningEndpoint);
            _listenSocket.Listen(_settings.MaxPendingConnections);
            if(_settings.KeepAlive)
            {
                _listenSocket.SetSocketOption(SocketOptionLevel.Tcp,SocketOptionName.KeepAlive, 5000);
            }

            StartAccept();
        }

        private SocketAsyncEventArgs CreateNewSocketAsyncEventArgs(GenericPool<SocketAsyncEventArgs> pool)
        {
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);

            AcceptOpUserToken theAcceptOpToken = new AcceptOpUserToken(pool.AssignTokenId() + 10000);
            acceptEventArg.UserToken = theAcceptOpToken;

            return acceptEventArg;
        }

        internal void StartAccept()
        {
            SocketAsyncEventArgs acceptEventArg;

            if (_acceptEventArgsPool.Count > 1)
            {
                try
                {
                    acceptEventArg = _acceptEventArgsPool.Pop();
                }
                catch
                {
                    acceptEventArg = CreateNewSocketAsyncEventArgs(_acceptEventArgsPool);
                }
            }
            else
            {
                acceptEventArg = CreateNewSocketAsyncEventArgs(_acceptEventArgsPool);
            }

            AcceptOpUserToken acceptOpToken = (AcceptOpUserToken)acceptEventArg.UserToken;
            _log.Info($"Started accepting socket with tokenId {acceptOpToken.TokenId}");

            _maxAcceptedClients.WaitOne();

            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                acceptOpToken = (AcceptOpUserToken)acceptEventArg.UserToken;
                _log.Info($"StartAccept in if (!willRaiseEvent), accept token id {acceptOpToken.TokenId}");

                ProcessAccept(acceptEventArg);
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            AcceptOpUserToken acceptOpToken;
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                StartAccept();

                acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
                _log.Error($"Error during accepting socket with tokenId {acceptOpToken.TokenId}");

                HandleBadAccept(acceptEventArgs);

                return;
            }

            Int32 numberOfConnectedSockets = Interlocked.Increment(ref _acceptedSockets);
            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
            _log.Info($"Processing accepting socket with tokenId {acceptOpToken.TokenId}, total concurrent accepted sockets is {numberOfConnectedSockets}");

            StartAccept();

            IClientHandler clientHandler = _clientHandlersPool.Pop();
            
            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;

            _log.Info("Accept id " + acceptOpToken.TokenId + ". client(s) connected = " + _acceptedSockets);

            clientHandler.AcceptSocket(acceptEventArgs.AcceptSocket);

            acceptEventArgs.AcceptSocket = null;
            _acceptEventArgsPool.Push(acceptEventArgs);

            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
            _log.Info($"Back to {nameof(_acceptEventArgsPool)} goes accept id {acceptOpToken.TokenId}");

        }

        private void HandleBadAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            var acceptOpToken = (acceptEventArgs.UserToken as AcceptOpUserToken);
            if (acceptOpToken != null)
            {
                _log.Info($"Closing socket with tokenId {acceptOpToken.TokenId}");
            }

            acceptEventArgs.AcceptSocket.Close();

            _acceptEventArgsPool.Push(acceptEventArgs);
        }

        private void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            AcceptOpUserToken acceptOpToken = (AcceptOpUserToken)e.UserToken;

            _log.Debug($"Accept Socket with tokenId {acceptOpToken.TokenId} completed");

            ProcessAccept(e);
        }
    }
}
