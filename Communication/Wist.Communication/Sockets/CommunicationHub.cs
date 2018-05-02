using CommonServiceLocator;
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

namespace Wist.Communication.Sockets
{
    [RegisterDefaultImplementation(typeof(ICommunicationHub), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class CommunicationHub : ICommunicationHub
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(CommunicationHub));
        private readonly IBufferManager _bufferManager;

        private Semaphore _maxConnectedClients;
        private GenericPool<SocketAsyncEventArgs> _acceptEventArgsPool;
        private GenericPool<IClientHandler> _clientHandlersPool;
        private readonly List<IClientHandler> _clientConnectedList;
        private SocketListenerSettings _settings;
        private Socket _listenSocket;
        private int _connectedSockets = 0;
        private ICommunicationProvisioning _communicationProvisioning = null;

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
            _clientConnectedList = new List<IClientHandler>();
        }

        #region ICommunicationHub implementation

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and 
        /// context objects.  These objects do not need to be preallocated 
        /// or reused, but it is done this way to illustrate how the API can 
        /// easily be used to create reusable objects to increase server performance.
        /// </summary>
        public void Init(SocketListenerSettings settings, ICommunicationProvisioning communicationProvisioning = null)
        {
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

            // Allocates one large byte buffer which all I/O operations use a piece of.  This guards against memory fragmentation
            _bufferManager.InitBuffer(_settings.ReceiveBufferSize * _settings.MaxConnections * _settings.OpsToPreAllocate, _settings.ReceiveBufferSize);
            _acceptEventArgsPool = new GenericPool<SocketAsyncEventArgs>(_settings.MaxSimultaneousAcceptOps);
            _clientHandlersPool = new GenericPool<IClientHandler>(_settings.MaxConnections);
            _maxConnectedClients = new Semaphore(_settings.MaxConnections, _settings.MaxConnections);

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

        public void StartListen()
        {
            _listenSocket = new Socket(_settings.ListeningEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_settings.ListeningEndpoint);
            _listenSocket.Listen(_settings.MaxPendingConnections);
            if (_settings.KeepAlive)
            {
                _listenSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, 5000);
            }

            StartAccept();
        }

        #endregion ICommunicationHub implementation

        #region Private functions

        private void CommunicationProvisioning_AllowedEndpointsChanged(object sender, EventArgs e)
        {
            List<IClientHandler> clientsToBeDisconnected = _clientConnectedList.Where(c => _communicationProvisioning.AllowedEndpoints.All(ep => !ep.Address.Equals(c.RemoteEndPoint.Address))).ToList();

            foreach (IClientHandler clientHandler in clientsToBeDisconnected)
            {
                clientHandler.Close();
            }

            while (_clientConnectedList.Any(c => clientsToBeDisconnected.Contains(c)))
            {
                Thread.Sleep(100);
            };

            foreach (IPEndPoint endPoint in _communicationProvisioning.AllowedEndpoints.Where(ep => _clientConnectedList.All(cc => !cc.RemoteEndPoint.Address.Equals(ep.Address))))
            {
                IClientHandler clientHandler = _clientHandlersPool.Pop();

                Interlocked.Increment(ref _connectedSockets);

                _log.Info($"Socket with tokenId {clientHandler.TokenId} disconnected. {_connectedSockets} client(s) connected.");

                try
                {
                    clientHandler.Connect(endPoint);
                }
                catch
                {
                    ReleaseClientHandler(clientHandler);
                }
            }
        }

        private void ClientHandler_SocketClosedEvent(object sender, EventArgs e)
        {
            IClientHandler clientHandler = (IClientHandler)sender;
            _clientConnectedList.Remove(clientHandler);
            ReleaseClientHandler(clientHandler);
        }

        private void ReleaseClientHandler(IClientHandler clientHandler)
        {
            _clientHandlersPool.Push(clientHandler);

            Interlocked.Decrement(ref _connectedSockets);

            _log.Info($"Socket with tokenId {clientHandler.TokenId} disconnected. {_connectedSockets} client(s) connected.");

            _maxConnectedClients.Release();
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

            _maxConnectedClients.WaitOne();

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
            if (_communicationProvisioning != null)
            {
                if (!_communicationProvisioning.AllowedEndpoints.Any(ep => ep.Address.Equals(((IPEndPoint)acceptEventArgs.RemoteEndPoint).Address)))
                {
                    HandleBadAccept(acceptEventArgs);
                    return;
                }
            }

            AcceptOpUserToken acceptOpToken;
            if (acceptEventArgs.SocketError != SocketError.Success)
            {
                StartAccept();

                acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
                _log.Error($"Error during accepting socket with tokenId {acceptOpToken.TokenId}");

                HandleBadAccept(acceptEventArgs);
            }
            else
            {
                StartAccept();

                InitializeClientHandler(acceptEventArgs);
            }
        }

        private void InitializeClientHandler(SocketAsyncEventArgs acceptEventArgs)
        {
            AcceptOpUserToken acceptOpToken;
            IClientHandler clientHandler = _clientHandlersPool.Pop();

            Int32 numberOfConnectedSockets = Interlocked.Increment(ref _connectedSockets);
            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
            _log.Info($"Processing accepting socket with tokenId {acceptOpToken.TokenId}, total concurrent accepted sockets is {numberOfConnectedSockets}");

            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;

            _log.Info("Accept id " + acceptOpToken.TokenId + ". client(s) connected = " + _connectedSockets);

            try
            {
                clientHandler.AcceptSocket(acceptEventArgs.AcceptSocket);
                _clientConnectedList.Add(clientHandler);
            }
            catch
            {
                ReleaseClientHandler(clientHandler);
            }
            finally
            {
                acceptEventArgs.AcceptSocket = null;
                _acceptEventArgsPool.Push(acceptEventArgs);

                acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
                _log.Info($"Back to {nameof(_acceptEventArgsPool)} goes accept id {acceptOpToken.TokenId}");
            }
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

        #endregion Private functions
    }
}
