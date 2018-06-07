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
using Wist.BlockLattice.Core.DataModel;
using Wist.BlockLattice.Core.Interfaces;
using System.Threading.Tasks;

namespace Wist.Communication.Sockets
{
    [RegisterDefaultImplementation(typeof(ICommunicationServer), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class CommunicationServerBase : ICommunicationServer
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(CommunicationServerBase));
        private readonly IBufferManager _bufferManager;
        private readonly IPacketsHandler _packetsHandler;
        private readonly IPacketSerializersFactory _packetSerializersFactory;
        private Semaphore _maxConnectedClients;
        private GenericPool<SocketAsyncEventArgs> _acceptEventArgsPool;
        private GenericPool<ICommunicationChannel> _communicationChannelsPool;
        private readonly List<ICommunicationChannel> _clientConnectedList;
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
        public CommunicationServerBase(IBufferManager bufferManager, IPacketSerializersFactory packetSerializersFactory, IPacketsHandler packetsHandler)
        {
            _bufferManager = bufferManager;
            _packetSerializersFactory = packetSerializersFactory;
            _packetsHandler = packetsHandler;
            _clientConnectedList = new List<ICommunicationChannel>();
        }

        #region ICommunicationHub implementation

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and 
        /// context objects.  These objects do not need to be preallocated 
        /// or reused, but it is done this way to illustrate how the API can 
        /// easily be used to create reusable objects to increase server performance.
        /// </summary>
        public void Init(SocketListenerSettings settings, IBlocksProcessor blocksProcessor, ICommunicationProvisioning communicationProvisioning = null)
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

            _packetsHandler?.Initialize(blocksProcessor);

            // Allocates one large byte buffer which all I/O operations use a piece of.  This guards against memory fragmentation
            _bufferManager.InitBuffer(_settings.ReceiveBufferSize * _settings.MaxConnections * _settings.OpsToPreAllocate, _settings.ReceiveBufferSize);
            _acceptEventArgsPool = new GenericPool<SocketAsyncEventArgs>(_settings.MaxSimultaneousAcceptOps);
            _communicationChannelsPool = new GenericPool<ICommunicationChannel>(_settings.MaxConnections);
            _maxConnectedClients = new Semaphore(_settings.MaxConnections, _settings.MaxConnections);

            for (Int32 i = 0; i < _settings.MaxSimultaneousAcceptOps; i++)
            {
                _acceptEventArgsPool.Push(CreateNewSocketAsyncEventArgs(_acceptEventArgsPool));
            }

            Int32 tokenId;

            for (int i = 0; i < _settings.MaxConnections; i++)
            {
                tokenId = _communicationChannelsPool.AssignTokenId() + 1000000;

                ICommunicationChannel communicationChannel = ServiceLocator.Current.GetInstance<ICommunicationChannel>();
                communicationChannel.SocketClosedEvent += ClientHandler_SocketClosedEvent;
                communicationChannel.Init(tokenId, _settings.ReceiveBufferSize, _settings.KeepAlive, _packetsHandler);
                _communicationChannelsPool.Push(communicationChannel);
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

        public Task BroadcastMessage(BlockBase message)
        {
            return Task.Run(() => {
                foreach (ICommunicationChannel clientHandler in _clientConnectedList)
                {
                    try
                    {
                        IPacketSerializer packetSerializer = _packetSerializersFactory.Create(message.ChainType, message.BlockType);
                        clientHandler.PostMessage(packetSerializer.GetBodyBytes(message));
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Error during broadcasting message", ex);
                    }
                }
            });
        }

        #endregion ICommunicationHub implementation

        #region Private functions

        private void CommunicationProvisioning_AllowedEndpointsChanged(object sender, EventArgs e)
        {
            List<ICommunicationChannel> clientsToBeDisconnected = _clientConnectedList.Where(c => _communicationProvisioning.AllowedEndpoints.All(ep => !ep.Address.Equals(c.RemoteEndPoint.Address))).ToList();

            foreach (ICommunicationChannel clientHandler in clientsToBeDisconnected)
            {
                clientHandler.Close();
            }

            while (_clientConnectedList.Any(c => clientsToBeDisconnected.Contains(c)))
            {
                Thread.Sleep(100);
            };

            foreach (IPEndPoint endPoint in _communicationProvisioning.AllowedEndpoints.Where(ep => _clientConnectedList.All(cc => !cc.RemoteEndPoint.Address.Equals(ep.Address))))
            {
                ICommunicationChannel communicationChannel = _communicationChannelsPool.Pop();

                Interlocked.Increment(ref _connectedSockets);

                _log.Info($"Socket with tokenId {communicationChannel.TokenId} disconnected. {_connectedSockets} client(s) connected.");

                try
                {
                    communicationChannel.Connect(endPoint);
                }
                catch
                {
                    ReleaseClientHandler(communicationChannel);
                }
            }
        }

        private void ClientHandler_SocketClosedEvent(object sender, EventArgs e)
        {
            ICommunicationChannel clientHandler = (ICommunicationChannel)sender;
            _clientConnectedList.Remove(clientHandler);
            ReleaseClientHandler(clientHandler);
        }

        private void ReleaseClientHandler(ICommunicationChannel clientHandler)
        {
            _communicationChannelsPool.Push(clientHandler);

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

                InitializeCommunicationChannel(acceptEventArgs);
            }
        }

        private void InitializeCommunicationChannel(SocketAsyncEventArgs acceptEventArgs)
        {
            AcceptOpUserToken acceptOpToken;
            ICommunicationChannel communicationChannel = _communicationChannelsPool.Pop();

            Int32 numberOfConnectedSockets = Interlocked.Increment(ref _connectedSockets);
            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
            _log.Info($"Processing accepting socket with tokenId {acceptOpToken.TokenId}, total concurrent accepted sockets is {numberOfConnectedSockets}");

            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;

            _log.Info("Accept id " + acceptOpToken.TokenId + ". client(s) connected = " + _connectedSockets);

            try
            {
                communicationChannel.AcceptSocket(acceptEventArgs.AcceptSocket);
                _clientConnectedList.Add(communicationChannel);
            }
            catch
            {
                ReleaseClientHandler(communicationChannel);
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
