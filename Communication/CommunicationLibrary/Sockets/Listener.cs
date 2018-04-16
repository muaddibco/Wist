using CommonServiceLocator;
using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CommunicationLibrary.Sockets
{
    public class Listener
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(Listener));
        private readonly BufferManager _bufferManager;
        private readonly Semaphore _maxAcceptedClients;
        private readonly SocketListenerSettings _settings;
        private readonly SocketAsyncEventArgsPool _acceptEventArgsPool;
        private readonly SocketAsyncEventArgsPool _receiveSendEventArgsPool;

        private Socket _listenSocket;
        private int _acceptedSockets = 0;

        /// <summary>
        /// Create an uninitialized server instance.  
        /// To start the server listening for connection requests
        /// call the Init method followed by Start method 
        /// </summary>
        /// <param name="settings">instance of <see cref="SocketListenerSettings"/> with defined settings of listener</param>
        /// <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public Listener(SocketListenerSettings settings)
        {
            _settings = settings;
            _bufferManager = new BufferManager(_settings.ReceiveBufferSize * _settings.MaxConnections * _settings.OpsToPreAllocate, _settings.ReceiveBufferSize * _settings.OpsToPreAllocate);
            _acceptEventArgsPool = new SocketAsyncEventArgsPool(_settings.MaxSimultaneousAcceptOps);
            _receiveSendEventArgsPool = new SocketAsyncEventArgsPool(_settings.MaxConnections);
            _maxAcceptedClients = new Semaphore(_settings.MaxConnections, _settings.MaxConnections);
        }

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and 
        /// context objects.  These objects do not need to be preallocated 
        /// or reused, but it is done this way to illustrate how the API can 
        /// easily be used to create reusable objects to increase server performance.
        /// </summary>
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            _bufferManager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs readWriteEventArg;

            for (Int32 i = 0; i < _settings.MaxSimultaneousAcceptOps; i++)
            {
                _acceptEventArgsPool.Push(CreateNewSocketAsyncEventArgs(_acceptEventArgsPool));
            }

            Int32 tokenId;

            for (int i = 0; i < _settings.MaxConnections; i++)
            {
                tokenId = _receiveSendEventArgsPool.AssignTokenId() + 1000000;
                
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                DataHoldingUserToken token = ServiceLocator.Current.GetInstance<DataHoldingUserToken>();
                token.Init(tokenId, readWriteEventArg.Offset, readWriteEventArg.Offset + _settings.ReceiveBufferSize);
                readWriteEventArg.UserToken = token;

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                _bufferManager.SetBuffer(readWriteEventArg);


                // add SocketAsyncEventArg to the pool
                _receiveSendEventArgsPool.Push(readWriteEventArg);
            }
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

        private SocketAsyncEventArgs CreateNewSocketAsyncEventArgs(SocketAsyncEventArgsPool pool)
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

            //Get a SocketAsyncEventArgs object to accept the connection.                        
            //Get it from the pool if there is more than one in the pool.
            //We could use zero as bottom, but one is a little safer.            
            if (_acceptEventArgsPool.Count > 1)
            {
                try
                {
                    acceptEventArg = _acceptEventArgsPool.Pop();
                }
                //or make a new one.
                catch
                {
                    acceptEventArg = CreateNewSocketAsyncEventArgs(_acceptEventArgsPool);
                }
            }
            //or make a new one.
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

            SocketAsyncEventArgs receiveSendEventArgs = _receiveSendEventArgsPool.Pop();

            receiveSendEventArgs.AcceptSocket = acceptEventArgs.AcceptSocket;

            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
            _log.Info("Accept id " + acceptOpToken.TokenId + ". RecSend id " + ((DataHoldingUserToken)receiveSendEventArgs.UserToken).TokenId + ".  Remote endpoint = " + IPAddress.Parse(((IPEndPoint)receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Address.ToString()) + ": " + ((IPEndPoint)receiveSendEventArgs.AcceptSocket.RemoteEndPoint).Port.ToString() + ". client(s) connected = " + _acceptedSockets);

            acceptEventArgs.AcceptSocket = null;
            _acceptEventArgsPool.Push(acceptEventArgs);

            acceptOpToken = (AcceptOpUserToken)acceptEventArgs.UserToken;
            _log.Info($"Back to {nameof(_acceptEventArgsPool)} goes accept id {acceptOpToken.TokenId}");

            StartReceive(receiveSendEventArgs);
        }

        private void StartReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            _log.Info($"Start receive for tokenId {receiveSendToken.TokenId}");

            receiveSendEventArgs.SetBuffer(receiveSendToken.BufferOffsetReceive, _settings.ReceiveBufferSize);

            bool willRaiseEvent = receiveSendEventArgs.AcceptSocket.ReceiveAsync(receiveSendEventArgs);

            if (!willRaiseEvent)
            {
                _log.Info($"StartReceive in if (!willRaiseEvent), tokenId {receiveSendToken.TokenId}");

                ProcessReceive(receiveSendEventArgs);
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveSendEventArgs)
        {
            DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)receiveSendEventArgs.UserToken;
            // If there was a socket error, close the connection. This is NOT a normal
            // situation, if you get an error here.
            // In the Microsoft example code they had this error situation handled
            // at the end of ProcessReceive. Putting it here improves readability
            // by reducing nesting some.
            if (receiveSendEventArgs.SocketError != SocketError.Success)
            {
                _log.Error($"ProcessReceive ERROR, tokenId {receiveSendToken.TokenId}");

                //receiveSendToken.Reset();
                CloseClientSocket(receiveSendEventArgs);

                return;
            }

            // If no data was received, close the connection. This is a NORMAL
            // situation that shows when the client has finished sending data.
            if (receiveSendEventArgs.BytesTransferred == 0)
            {
                _log.Info($"ProcessReceive NO DATA, receiveSendToken id {receiveSendToken.TokenId}");

                if (!_settings.KeepAlive)
                {
                    CloseClientSocket(receiveSendEventArgs);
                }
                return;
            }

            //The BytesTransferred property tells us how many bytes 
            //we need to process.
            Int32 remainingBytesToProcess = receiveSendEventArgs.BytesTransferred;

            _log.Info($"ProcessReceive {receiveSendToken.TokenId}. remainingBytesToProcess = {remainingBytesToProcess}");

            receiveSendToken.ClientHandler.PushBuffer(receiveSendEventArgs.Buffer, receiveSendEventArgs.BytesTransferred);

            StartReceive(receiveSendEventArgs);
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            var receiveSendToken = (e.UserToken as DataHoldingUserToken);

            _log.Info($"Closing client socket with tokenId {receiveSendToken.TokenId}");

            try
            {
                _log.Info($"Trying shutdown for tokenId {receiveSendToken.TokenId}");
                e.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                _log.Warn($"Socket shutdown failed, tokenId {receiveSendToken.TokenId}", ex);
            }

            e.AcceptSocket.Close();

            _receiveSendEventArgsPool.Push(e);

            Interlocked.Decrement(ref _acceptedSockets);

            _log.Info($"Socket with tokenId {receiveSendToken.TokenId} disconnected. {_acceptedSockets} client(s) connected.");

            _maxAcceptedClients.Release();
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

        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            DataHoldingUserToken receiveSendToken = (DataHoldingUserToken)e.UserToken;

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    _log.Info($"IO_Completed method in Receive, receiveSendToken id {receiveSendToken.TokenId}");
                    ProcessReceive(e);
                    break;

                case SocketAsyncOperation.Send:
                    _log.Info($"IO_Completed method in Send, id {receiveSendToken.TokenId}");

                    //ProcessSend(e);
                    break;

                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }
        }
    }
}
